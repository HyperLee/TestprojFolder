// 匯率計算器前端邏輯

$(document).ready(function () {
    // 即時金額驗證
    $('input[type="number"]').on('input', function () {
        const value = parseFloat($(this).val());
        const $input = $(this);
        
        // 移除舊的錯誤樣式
        $input.removeClass('is-invalid is-valid');
        
        // 即時驗證
        if ($input.val() !== '') {
            if (isNaN(value) || value <= 0) {
                $input.addClass('is-invalid');
                showInputError($input, '請輸入大於 0 的金額');
            } else {
                $input.addClass('is-valid');
                hideInputError($input);
            }
        } else {
            hideInputError($input);
        }
    });

    // 離開焦點時驗證
    $('input[type="number"]').on('blur', function () {
        const value = parseFloat($(this).val());
        const $input = $(this);
        
        if ($input.val() === '') {
            $input.addClass('is-invalid');
            showInputError($input, '此欄位為必填');
        } else if (isNaN(value) || value <= 0) {
            $input.addClass('is-invalid');
            showInputError($input, '請輸入大於 0 的金額');
        }
    });

    // 表單提交前驗證
    $('form').on('submit', function (e) {
        let isValid = true;

        // 驗證金額（只針對計算表單）
        if ($(this).find('input[type="number"]').length > 0) {
            const $amountInput = $(this).find('input[type="number"]');
            const amount = parseFloat($amountInput.val());
            
            if ($amountInput.val() === '' || isNaN(amount) || amount <= 0) {
                $amountInput.addClass('is-invalid');
                showInputError($amountInput, '請輸入有效的金額');
                isValid = false;
            } else {
                $amountInput.removeClass('is-invalid').addClass('is-valid');
                hideInputError($amountInput);
            }
        }

        if (!isValid) {
            e.preventDefault();
            // 聚焦到第一個錯誤欄位
            $('.is-invalid').first().focus();
            return false;
        }

        // 顯示載入狀態
        const $button = $(this).find('button[type="submit"]');
        const originalText = $button.html();
        $button.prop('disabled', true).addClass('loading');
        
        // 更新匯率按鈕：防止30秒內重複點擊
        if ($(this).attr('method') === 'post' && $(this).attr('asp-page-handler') === 'UpdateRates') {
            setTimeout(function() {
                $button.prop('disabled', false).removeClass('loading');
                $button.html(originalText);
            }, 30000);
        }
    });

    // 貨幣選擇改變時的提示
    $('select').on('change', function () {
        $(this).addClass('is-valid');
        setTimeout(() => {
            $(this).removeClass('is-valid');
        }, 1000);
    });

    // 自動關閉成功訊息
    setTimeout(function () {
        $('.alert-success.alert-dismissible').fadeOut('slow', function () {
            $(this).remove();
        });
    }, 5000);

    // 鍵盤導航增強
    $('input, select, button').on('keydown', function (e) {
        // Enter 鍵跳轉到下一個輸入欄位
        if (e.key === 'Enter' && $(this).is('input, select')) {
            e.preventDefault();
            const inputs = $('input, select, button').filter(':visible');
            const currentIndex = inputs.index(this);
            if (currentIndex < inputs.length - 1) {
                inputs.eq(currentIndex + 1).focus();
            }
        }
    });

    // 數字輸入格式化
    $('input[type="number"]').on('blur', function () {
        const value = parseFloat($(this).val());
        if (!isNaN(value) && value > 0) {
            // 格式化為兩位小數
            $(this).val(value.toFixed(2));
        }
    });

    // 輔助函式：顯示輸入錯誤
    function showInputError($input, message) {
        let $errorSpan = $input.next('.invalid-feedback');
        if ($errorSpan.length === 0) {
            $errorSpan = $('<span class="invalid-feedback d-block"></span>');
            $input.after($errorSpan);
        }
        $errorSpan.text(message).show();
    }

    // 輔助函式：隱藏輸入錯誤
    function hideInputError($input) {
        $input.next('.invalid-feedback').hide();
    }

    // 無障礙：為結果添加 ARIA live region
    $('.alert-success').attr('role', 'status').attr('aria-live', 'polite');
    $('.alert-warning, .alert-danger').attr('role', 'alert').attr('aria-live', 'assertive');

    // 工具提示初始化
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
});
