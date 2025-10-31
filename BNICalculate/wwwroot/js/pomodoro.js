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
 * T093-T095: 更新圓形進度環
 * @param {number} remainingSeconds - 剩餘秒數
 * @param {number} totalSeconds - 總秒數
 * @param {string} sessionType - 時段類型 ('work' 或 'break')
 */
function updateProgressRing(remainingSeconds, totalSeconds, sessionType) {
    const circle = document.getElementById('progress-ring-circle');
    if (!circle) return;
    
    // T093: 計算 circumference（圓周長 = 2 * π * r）
    const radius = 130; // 與 SVG 中的 r 屬性相同
    const circumference = 2 * Math.PI * radius; // ≈ 816.81
    
    // 計算進度百分比（已完成的部分）
    const progress = (totalSeconds - remainingSeconds) / totalSeconds;
    
    // T093: 更新 stroke-dashoffset（從滿圓開始逆向減少）
    const offset = circumference * (1 - progress);
    circle.style.strokeDashoffset = offset;
    
    // T095: 工作/休息時段顏色切換
    if (sessionType === 'work') {
        circle.classList.remove('break-phase');
        circle.classList.add('work-phase');
    } else {
        circle.classList.remove('work-phase');
        circle.classList.add('break-phase');
    }
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

/**
 * T059-T061: 記錄完成的時段至伺服器（含錯誤處理和重試）
 * @param {string} sessionType - 時段類型：'work' 或 'break'
 * @param {number} durationMinutes - 時長（分鐘）
 * @returns {Promise<boolean>} 是否成功記錄
 */
async function recordCompletedSession(sessionType, durationMinutes) {
    const maxRetries = 3;
    let attempt = 0;
    
    while (attempt < maxRetries) {
        try {
            const response = await fetch('/Pomodoro?handler=RecordComplete', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: JSON.stringify({
                    sessionType: sessionType,
                    durationMinutes: durationMinutes
                })
            });
            
            if (response.ok) {
                const data = await response.json();
                if (data.success) {
                    return data;
                }
            }
            
            throw new Error(`HTTP ${response.status}`);
        } catch (error) {
            attempt++;
            console.error(`記錄時段失敗（嘗試 ${attempt}/${maxRetries}）:`, error);
            
            if (attempt < maxRetries) {
                // 等待後重試（指數退避）
                await new Promise(resolve => setTimeout(resolve, 1000 * attempt));
            } else {
                // 最後一次嘗試失敗
                if (window.showNotification) {
                    showNotification('無法記錄統計資料，請檢查網路連線', 'danger');
                }
                return null;
            }
        }
    }
    
    return null;
}

/**
 * T066-T072: MultiWindowGuard 類別 - 多視窗衝突偵測
 */
class MultiWindowGuard {
    constructor() {
        this.lockKey = 'pomodoroWindowLock';
        this.heartbeatInterval = null;
        this.isMainWindow = false;
    }
    
    /**
     * T067: 嘗試取得主視窗鎖定
     * @returns {boolean} 是否成功取得鎖定
     */
    tryAcquireLock() {
        try {
            const now = Date.now();
            const lockData = localStorage.getItem(this.lockKey);
            
            if (lockData) {
                const lock = JSON.parse(lockData);
                const timeSinceHeartbeat = now - lock.lastHeartbeat;
                
                // T067: 檢查心跳，若超過 5 秒視為過期
                if (timeSinceHeartbeat < 5000) {
                    // 另一個視窗正在運作
                    return false;
                }
            }
            
            // 設定鎖定
            this._setLock(now);
            this.isMainWindow = true;
            
            // T068: 啟動心跳機制
            this.startHeartbeat();
            
            return true;
        } catch (e) {
            console.error('無法取得視窗鎖定:', e);
            return false;
        }
    }
    
    /**
     * T068: 啟動心跳機制（每 2 秒更新）
     */
    startHeartbeat() {
        this.heartbeatInterval = setInterval(() => {
            if (this.isMainWindow) {
                this._setLock(Date.now());
            }
        }, 2000);
    }
    
    /**
     * T069: 釋放鎖定
     */
    releaseLock() {
        try {
            if (this.heartbeatInterval) {
                clearInterval(this.heartbeatInterval);
                this.heartbeatInterval = null;
            }
            
            if (this.isMainWindow) {
                localStorage.removeItem(this.lockKey);
                this.isMainWindow = false;
            }
        } catch (e) {
            console.error('無法釋放視窗鎖定:', e);
        }
    }
    
    /**
     * 設定鎖定資料
     * @private
     */
    _setLock(timestamp) {
        try {
            localStorage.setItem(this.lockKey, JSON.stringify({
                lastHeartbeat: timestamp,
                windowId: Math.random().toString(36).substring(7)
            }));
        } catch (e) {
            console.error('無法儲存視窗鎖定:', e);
        }
    }
}
