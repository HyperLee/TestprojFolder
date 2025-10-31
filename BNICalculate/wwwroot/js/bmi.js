// BMI 計算器 JavaScript

/**
 * 當頁面載入完成時初始化
 */
document.addEventListener('DOMContentLoaded', function() {
    const calculateBtn = document.getElementById('calculate-btn');
    const clearBtn = document.getElementById('clear-btn');
    
    if (calculateBtn) {
        calculateBtn.addEventListener('click', calculateBMI);
    }
    
    if (clearBtn) {
        clearBtn.addEventListener('click', clearForm);
    }
});

/**
 * 計算 BMI 並顯示結果
 */
function calculateBMI() {
    // 清除之前的錯誤訊息
    clearErrors();
    
    // 讀取輸入值
    const heightInput = document.getElementById('height');
    const weightInput = document.getElementById('weight');
    
    const height = parseFloat(heightInput.value);
    const weight = parseFloat(weightInput.value);
    
    // 驗證輸入
    if (!validateInput(height, weight, heightInput, weightInput)) {
        return;
    }
    
    // 計算 BMI
    const bmi = weight / (height * height);
    
    // 四捨五入至小數點一位
    const bmiRounded = Math.round(bmi * 10) / 10;
    
    // 取得 BMI 分類
    const category = getBMICategory(bmiRounded);
    
    // 顯示結果
    displayResult(bmiRounded, category);
}

/**
 * 取得 BMI 分類（WHO 標準）
 * @param {number} bmi - BMI 值
 * @returns {string} BMI 分類
 */
function getBMICategory(bmi) {
    if (bmi < 18.5) {
        return '過輕';
    } else if (bmi >= 18.5 && bmi <= 23.9) {
        return '正常';
    } else if (bmi >= 24.0 && bmi <= 26.9) {
        return '過重';
    } else if (bmi >= 27.0 && bmi <= 29.9) {
        return '輕度肥胖';
    } else if (bmi >= 30.0 && bmi <= 34.9) {
        return '中度肥胖';
    } else {
        return '重度肥胖';
    }
}

/**
 * 顯示計算結果
 * @param {number} bmi - BMI 值
 * @param {string} category - BMI 分類
 */
function displayResult(bmi, category) {
    const bmiValueElement = document.getElementById('bmi-value');
    const bmiCategoryElement = document.getElementById('bmi-category');
    const resultContainer = document.getElementById('result-container');
    
    if (bmiValueElement && bmiCategoryElement && resultContainer) {
        bmiValueElement.textContent = `BMI: ${bmi}`;
        bmiCategoryElement.textContent = `分類: ${category}`;
        resultContainer.style.display = 'block';
    }
}

/**
 * 驗證輸入
 * @param {number} height - 身高
 * @param {number} weight - 體重
 * @param {HTMLElement} heightInput - 身高輸入欄位
 * @param {HTMLElement} weightInput - 體重輸入欄位
 * @returns {boolean} 驗證是否通過
 */
function validateInput(height, weight, heightInput, weightInput) {
    let isValid = true;
    
    // 檢查空值
    if (!heightInput.value || !weightInput.value) {
        displayError('form-group', '請輸入完整的身高和體重資料');
        return false;
    }
    
    // 檢查數字格式
    if (isNaN(height)) {
        displayError(heightInput.parentElement, '請輸入數字');
        isValid = false;
    }
    
    if (isNaN(weight)) {
        displayError(weightInput.parentElement, '請輸入數字');
        isValid = false;
    }
    
    // 檢查正數
    if (!isNaN(height) && height <= 0) {
        displayError(heightInput.parentElement, '請輸入有效的身高值（大於 0）');
        isValid = false;
    }
    
    if (!isNaN(weight) && weight <= 0) {
        displayError(weightInput.parentElement, '請輸入有效的體重值（大於 0）');
        isValid = false;
    }
    
    return isValid;
}

/**
 * 顯示錯誤訊息
 * @param {HTMLElement|string} field - 欄位元素或選擇器
 * @param {string} message - 錯誤訊息
 */
function displayError(field, message) {
    const fieldElement = typeof field === 'string' ? document.querySelector(`.${field}`) : field;
    
    if (fieldElement) {
        // 檢查是否已存在錯誤訊息
        const existingError = fieldElement.querySelector('.error-message');
        if (!existingError) {
            const errorSpan = document.createElement('span');
            errorSpan.className = 'error-message';
            errorSpan.textContent = message;
            fieldElement.appendChild(errorSpan);
        }
    }
}

/**
 * 清除所有錯誤訊息
 */
function clearErrors() {
    const errorMessages = document.querySelectorAll('.error-message');
    errorMessages.forEach(error => error.remove());
}

/**
 * 清除表單
 */
function clearForm() {
    // 清空輸入欄位
    const heightInput = document.getElementById('height');
    const weightInput = document.getElementById('weight');
    
    if (heightInput) heightInput.value = '';
    if (weightInput) weightInput.value = '';
    
    // 清空結果顯示
    const bmiValueElement = document.getElementById('bmi-value');
    const bmiCategoryElement = document.getElementById('bmi-category');
    const resultContainer = document.getElementById('result-container');
    
    if (bmiValueElement) bmiValueElement.textContent = '';
    if (bmiCategoryElement) bmiCategoryElement.textContent = '';
    if (resultContainer) resultContainer.style.display = 'none';
    
    // 清除錯誤訊息
    clearErrors();
}
