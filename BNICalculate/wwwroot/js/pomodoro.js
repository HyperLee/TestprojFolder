/**
 * 番茄工作法計時器 - 客戶端邏輯
 * 提供核心計時功能、狀態管理、頁面恢復
 */

/**
 * PomodoroTimer 類別 - 核心計時器邏輯
 */
class PomodoroTimer {
    /**
     * 建立番茄工作法計時器實例
     * @param {Object} options - 設定選項
     * @param {number} options.workDuration - 工作時長（分鐘），預設 25
     * @param {number} options.breakDuration - 休息時長（分鐘），預設 5
     * @param {Function} options.onTick - 每秒回呼函式 (remainingSeconds) => void
     * @param {Function} options.onWorkComplete - 工作完成回呼函式 () => void
     * @param {Function} options.onBreakComplete - 休息完成回呼函式 () => void
     * @param {Function} options.onStateChange - 狀態變更回呼函式 (state) => void
     */
    constructor(options = {}) {
        this.workDuration = options.workDuration || 25;
        this.breakDuration = options.breakDuration || 5;
        this.onTick = options.onTick || (() => {});
        this.onWorkComplete = options.onWorkComplete || (() => {});
        this.onBreakComplete = options.onBreakComplete || (() => {});
        this.onStateChange = options.onStateChange || (() => {});
        
        // 狀態屬性
        this.state = 'idle'; // 'idle', 'running', 'paused'
        this.sessionType = 'work'; // 'work', 'break'
        this.startTimestamp = null;
        this.totalDuration = this.workDuration * 60; // 秒數
        this.remainingSeconds = this.totalDuration;
        this.intervalId = null;
    }
    
    /**
     * 開始工作時段（T018）
     */
    startWork() {
        if (this.state === 'running') {
            throw new Error('計時器已在執行中');
        }
        
        this.sessionType = 'work';
        this.totalDuration = this.workDuration * 60;
        this.remainingSeconds = this.totalDuration;
        this.startTimestamp = Date.now();
        this.state = 'running';
        
        this._startInterval();
        this.saveState();
        this.onStateChange(this.state);
    }
    
    /**
     * 開始休息時段（T019）
     */
    startBreak() {
        if (this.state === 'running') {
            this.reset();
        }
        
        this.sessionType = 'break';
        this.totalDuration = this.breakDuration * 60;
        this.remainingSeconds = this.totalDuration;
        this.startTimestamp = Date.now();
        this.state = 'running';
        
        this._startInterval();
        this.saveState();
        this.onStateChange(this.state);
    }
    
    /**
     * 暫停計時器（Phase 4 - User Story 2）
     */
    pause() {
        if (this.state !== 'running') {
            throw new Error('計時器未執行');
        }
        
        clearInterval(this.intervalId);
        this.intervalId = null;
        
        // 計算剩餘時間（T020 - Date.now() 校準）
        const elapsed = Math.floor((Date.now() - this.startTimestamp) / 1000);
        this.remainingSeconds = Math.max(0, this.totalDuration - elapsed);
        
        this.state = 'paused';
        this.startTimestamp = null;
        
        this.saveState();
        this.onStateChange(this.state);
    }
    
    /**
     * 繼續計時（Phase 4 - User Story 2）
     */
    resume() {
        if (this.state !== 'paused') {
            throw new Error('計時器未暫停');
        }
        
        this.totalDuration = this.remainingSeconds;
        this.startTimestamp = Date.now();
        this.state = 'running';
        
        this._startInterval();
        this.saveState();
        this.onStateChange(this.state);
    }
    
    /**
     * 重置計時器（Phase 4 - User Story 2）
     */
    reset() {
        if (this.intervalId) {
            clearInterval(this.intervalId);
            this.intervalId = null;
        }
        
        this.state = 'idle';
        this.sessionType = 'work';
        this.startTimestamp = null;
        this.totalDuration = this.workDuration * 60;
        this.remainingSeconds = this.totalDuration;
        
        localStorage.removeItem('pomodoroState');
        this.onStateChange(this.state);
    }
    
    /**
     * 啟動 setInterval 並實作 Date.now() 校準機制（T020）
     * @private
     */
    _startInterval() {
        this.intervalId = setInterval(() => {
            // T020: Date.now() 時間校準機制 - 每次 tick 重新計算剩餘時間
            const elapsed = Math.floor((Date.now() - this.startTimestamp) / 1000);
            this.remainingSeconds = Math.max(0, this.totalDuration - elapsed);
            
            // T021: onTick 回呼觸發（每秒更新剩餘時間）
            this.onTick(this.remainingSeconds);
            
            // 檢查是否完成
            if (this.remainingSeconds === 0) {
                clearInterval(this.intervalId);
                this.intervalId = null;
                this.state = 'idle';
                
                if (this.sessionType === 'work') {
                    // T022: onWorkComplete 回呼觸發（工作時段結束）
                    this.onWorkComplete();
                } else {
                    // T023: onBreakComplete 回呼觸發（休息結束）
                    this.onBreakComplete();
                }
            }
        }, 1000);
    }
    
    /**
     * 儲存狀態至 localStorage（T031）
     */
    saveState() {
        const state = {
            isRunning: this.state === 'running',
            isPaused: this.state === 'paused',
            sessionType: this.sessionType,
            startTimestamp: this.startTimestamp,
            totalDuration: this.totalDuration,
            remainingSeconds: this.remainingSeconds,
            lastUpdateTimestamp: Date.now()
        };
        
        try {
            localStorage.setItem('pomodoroState', JSON.stringify(state));
        } catch (e) {
            console.error('無法儲存計時器狀態至 localStorage:', e);
        }
    }
    
    /**
     * 從 localStorage 載入狀態（T032）
     */
    loadState() {
        try {
            const json = localStorage.getItem('pomodoroState');
            if (!json) return false;
            
            const state = JSON.parse(json);
            
            // 檢查是否超過 5 分鐘未更新（視為過期）
            const timeSinceUpdate = Date.now() - state.lastUpdateTimestamp;
            if (timeSinceUpdate > 5 * 60 * 1000) {
                localStorage.removeItem('pomodoroState');
                return false;
            }
            
            // 恢復狀態
            this.sessionType = state.sessionType;
            this.totalDuration = state.totalDuration;
            
            if (state.isRunning && state.startTimestamp) {
                // 計算經過時間
                const elapsed = Math.floor((Date.now() - state.startTimestamp) / 1000);
                this.remainingSeconds = Math.max(0, state.totalDuration - elapsed);
                
                if (this.remainingSeconds > 0) {
                    // 恢復計時
                    this.startTimestamp = state.startTimestamp;
                    this.state = 'running';
                    this._startInterval();
                    this.onStateChange(this.state);
                    return true;
                } else {
                    // 時間已到，顯示完成通知
                    if (this.sessionType === 'work') {
                        this.onWorkComplete();
                    } else {
                        this.onBreakComplete();
                    }
                    localStorage.removeItem('pomodoroState');
                    return false;
                }
            } else if (state.isPaused) {
                // 恢復暫停狀態
                this.remainingSeconds = state.remainingSeconds;
                this.state = 'paused';
                this.onStateChange(this.state);
                return true;
            }
            
            return false;
        } catch (e) {
            console.error('無法載入計時器狀態:', e);
            localStorage.removeItem('pomodoroState');
            return false;
        }
    }
}

// 輔助函式（將在 Phase 3 UI 實作中使用）

/**
 * 格式化時間為 MM:SS（T028）
 * @param {number} seconds - 秒數
 * @returns {string} 格式化後的時間字串
 */
function formatTime(seconds) {
    const minutes = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
}

/**
 * 顯示 Bootstrap Toast 通知（T027）
 * @param {string} message - 通知訊息
 * @param {string} type - 通知類型：'success', 'info', 'warning', 'danger'
 */
function showNotification(message, type = 'info') {
    // 將在 Phase 3 UI 實作中完成
    console.log(`[${type.toUpperCase()}] ${message}`);
}
