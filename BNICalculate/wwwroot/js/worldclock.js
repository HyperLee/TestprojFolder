// 世界時鐘 JavaScript 邏輯

/**
 * 城市時區配置陣列 (依照 contracts/world-clock-data.md)
 */
const cityConfigs = [
    {
        id: "taipei",
        name: "台北",
        timeZone: "Asia/Taipei",
        offsetLabel: "GMT+8",
        hasDST: false
    },
    {
        id: "tokyo",
        name: "東京",
        timeZone: "Asia/Tokyo",
        offsetLabel: "GMT+9",
        hasDST: false
    },
    {
        id: "london",
        name: "倫敦",
        timeZone: "Europe/London",
        offsetLabel: "GMT+0/GMT+1",
        hasDST: true
    },
    {
        id: "new-york",
        name: "紐約",
        timeZone: "America/New_York",
        offsetLabel: "GMT-5/GMT-4",
        hasDST: true
    },
    {
        id: "los-angeles",
        name: "洛杉磯",
        timeZone: "America/Los_Angeles",
        offsetLabel: "GMT-8/GMT-7",
        hasDST: true
    },
    {
        id: "paris",
        name: "巴黎",
        timeZone: "Europe/Paris",
        offsetLabel: "GMT+1/GMT+2",
        hasDST: true
    },
    {
        id: "berlin",
        name: "柏林",
        timeZone: "Europe/Berlin",
        offsetLabel: "GMT+1/GMT+2",
        hasDST: true
    },
    {
        id: "moscow",
        name: "莫斯科",
        timeZone: "Europe/Moscow",
        offsetLabel: "GMT+3",
        hasDST: false
    },
    {
        id: "singapore",
        name: "新加坡",
        timeZone: "Asia/Singapore",
        offsetLabel: "GMT+8",
        hasDST: false
    },
    {
        id: "sydney",
        name: "悉尼",
        timeZone: "Australia/Sydney",
        offsetLabel: "GMT+10/GMT+11",
        hasDST: true
    }
];

/**
 * 時鐘狀態物件
 */
const clockState = {
    mainCity: cityConfigs[0], // 預設台北
    secondaryCities: cityConfigs.slice(1), // 其他 9 個城市
    isRunning: false,
    timerId: null,
    heartbeatTimerId: null, // 心跳檢測計時器
    lastUpdateTime: 0,
    formatters: {} // Formatter 實例快取
};

/**
 * 建立 DateTimeFormat 實例（重用優化）
 * @param {string} timeZone - IANA 時區識別符
 * @returns {Intl.DateTimeFormat} - 時間格式化器
 */
function createFormatter(timeZone) {
    if (!clockState.formatters[timeZone]) {
        try {
            clockState.formatters[timeZone] = {
                time: new Intl.DateTimeFormat('zh-TW', {
                    timeZone: timeZone,
                    hour: '2-digit',
                    minute: '2-digit',
                    second: '2-digit',
                    hour12: false
                }),
                date: new Intl.DateTimeFormat('zh-TW', {
                    timeZone: timeZone,
                    year: 'numeric',
                    month: '2-digit',
                    day: '2-digit'
                })
            };
        } catch (error) {
            console.error(`無法建立時區格式化器：${timeZone}`, error);
            throw error;
        }
    }
    return clockState.formatters[timeZone];
}

/**
 * 格式化時間為 HH:mm:ss
 * @param {Date} date - 日期物件
 * @param {string} timeZone - 時區
 * @returns {string} - 格式化的時間字串
 */
function formatTime(date, timeZone) {
    try {
        const formatter = createFormatter(timeZone);
        return formatter.time.format(date);
    } catch (error) {
        console.error('時間格式化錯誤', error);
        return '--:--:--';
    }
}

/**
 * 格式化日期為 YYYY-MM-DD
 * @param {Date} date - 日期物件
 * @param {string} timeZone - 時區
 * @returns {string} - 格式化的日期字串
 */
function formatDate(date, timeZone) {
    try {
        const formatter = createFormatter(timeZone);
        const parts = formatter.date.formatToParts(date);
        const year = parts.find(p => p.type === 'year').value;
        const month = parts.find(p => p.type === 'month').value;
        const day = parts.find(p => p.type === 'day').value;
        return `${year}-${month}-${day}`;
    } catch (error) {
        console.error('日期格式化錯誤', error);
        return '-----';
    }
}

/**
 * 更新主要時鐘顯示
 * @param {Date} now - 當前時間
 */
function updateMainClock(now) {
    const mainCity = clockState.mainCity;
    document.getElementById('main-city-name').textContent = mainCity.name;
    document.getElementById('main-timezone').textContent = mainCity.offsetLabel;
    document.getElementById('main-time').textContent = formatTime(now, mainCity.timeZone);
    document.getElementById('main-date').textContent = formatDate(now, mainCity.timeZone);
}

/**
 * 更新次要城市時鐘
 * @param {Date} now - 當前時間
 */
function updateSecondaryClock(now) {
    clockState.secondaryCities.forEach(city => {
        const cardElement = document.getElementById(`city-${city.id}`);
        if (cardElement) {
            const timeElement = cardElement.querySelector('.city-card-time');
            if (timeElement) {
                timeElement.textContent = formatTime(now, city.timeZone);
            }
        }
    });
}

/**
 * 更新所有時鐘
 */
function updateAllClocks() {
    try {
        const now = new Date();
        updateMainClock(now);
        updateSecondaryClock(now);
        clockState.lastUpdateTime = now.getTime();
    } catch (error) {
        console.error('更新時鐘時發生錯誤', error);
        showError();
    }
}

/**
 * 啟動時鐘
 */
function startClock() {
    if (clockState.isRunning) {
        return; // 避免重複啟動
    }
    
    clockState.isRunning = true;
    
    // 立即更新一次
    updateAllClocks();
    
    // 每秒更新
    clockState.timerId = setInterval(() => {
        updateAllClocks();
    }, 1000);
    
    console.log('時鐘已啟動');
}

/**
 * 停止時鐘
 */
function stopClock() {
    if (clockState.timerId) {
        clearInterval(clockState.timerId);
        clockState.timerId = null;
    }
    if (clockState.heartbeatTimerId) {
        clearInterval(clockState.heartbeatTimerId);
        clockState.heartbeatTimerId = null;
    }
    clockState.isRunning = false;
    console.log('時鐘已停止');
}

/**
 * 檢查心跳：驗證時鐘是否在 5 秒內有更新
 * @returns {boolean} - 心跳是否正常
 */
function checkHeartbeat() {
    const now = Date.now();
    const timeSinceLastUpdate = now - clockState.lastUpdateTime;
    
    // 如果超過 5 秒沒有更新，認為心跳失敗
    if (timeSinceLastUpdate > 5000) {
        console.error(`心跳檢測失敗：距離上次更新已過 ${timeSinceLastUpdate}ms`);
        return false;
    }
    
    return true;
}

/**
 * 心跳失敗時自動重啟時鐘
 */
function restartClockOnHeartbeatFailure() {
    console.error('偵測到時鐘停止運行，正在嘗試重新啟動...');
    
    // 停止現有計時器
    stopClock();
    
    // 重新啟動
    try {
        startClock();
        console.log('時鐘重新啟動成功');
    } catch (error) {
        console.error('時鐘重新啟動失敗', error);
        showError('時鐘已停止運行，請重新整理頁面。');
    }
}

/**
 * 啟動心跳檢測
 */
function startHeartbeatMonitor() {
    // 每 60 秒檢查一次心跳
    clockState.heartbeatTimerId = setInterval(() => {
        if (!checkHeartbeat()) {
            restartClockOnHeartbeatFailure();
        }
    }, 60000); // 60 秒
    
    console.log('心跳檢測已啟動');
}

/**
 * 顯示錯誤訊息
 * @param {string} message - 錯誤訊息（選擇性）
 */
function showError(message) {
    const errorElement = document.getElementById('error-message');
    if (errorElement) {
        if (message) {
            document.getElementById('error-text').textContent = message;
        }
        errorElement.classList.remove('d-none');
    }
    hideLoading();
    
    // 隱藏主要內容
    document.getElementById('main-clock')?.classList.add('d-none');
    document.getElementById('city-grid')?.classList.add('d-none');
}

/**
 * 隱藏載入指示器
 */
function hideLoading() {
    const loadingElement = document.getElementById('loading');
    if (loadingElement) {
        loadingElement.classList.add('d-none');
    }
}

/**
 * 顯示時鐘內容
 */
function showClockContent() {
    hideLoading();
    document.getElementById('main-clock')?.classList.remove('d-none');
    document.getElementById('city-grid')?.classList.remove('d-none');
}

/**
 * 初始化時鐘
 */
function initializeClock() {
    try {
        console.log('初始化世界時鐘...');
        
        // 檢查瀏覽器支援
        if (typeof Intl === 'undefined' || typeof Intl.DateTimeFormat === 'undefined') {
            throw new Error('瀏覽器不支援 Intl.DateTimeFormat API');
        }
        
        // 測試建立 formatter（驗證時區支援）
        cityConfigs.forEach(city => {
            createFormatter(city.timeZone);
        });
        
        // 顯示內容並啟動時鐘
        showClockContent();
        startClock();
        startHeartbeatMonitor(); // 啟動心跳檢測
        
        console.log('世界時鐘初始化完成');
    } catch (error) {
        console.error('初始化失敗', error);
        showError('無法載入時區資料，請檢查您的瀏覽器是否支援最新的國際化 API。');
    }
}

// 頁面載入完成後初始化
document.addEventListener('DOMContentLoaded', function() {
    initializeClock();
});

// 頁面卸載前清理計時器
window.addEventListener('beforeunload', function() {
    stopClock();
});
