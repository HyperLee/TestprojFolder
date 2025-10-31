// 匯率計算器前端邏輯

$(document).ready(function () {
    // 金額輸入格式驗證
    $('input[type="number"]').on('blur', function () {
        const value = parseFloat($(this).val());
        if (isNaN(value) || value <= 0) {
            $(this).addClass('is-invalid');
        } else {
            $(this).removeClass('is-invalid');
        }
    });

    // 表單提交前驗證
    $('form').on('submit', function (e) {
        let isValid = true;

        // 驗證金額
        const amountInput = $(this).find('input[type="number"]');
        const amount = parseFloat(amountInput.val());
        
        if (isNaN(amount) || amount <= 0) {
            amountInput.addClass('is-invalid');
            isValid = false;
        } else {
            amountInput.removeClass('is-invalid');
        }

        if (!isValid) {
            e.preventDefault();
            return false;
        }
    });

    // 清除錯誤樣式
    $('input').on('input', function () {
        $(this).removeClass('is-invalid');
    });
});
