// wwwroot/js/Share/Currency.js

// Tỷ giá dự phòng (nếu API lỗi)
let currentRates = { "VND": 1, "USD": 1 / 25400, "EUR": 1 / 27500 };
let currentSelectedCurrency = "VND"; // Tiền tệ mặc định

// Gọi API lấy tỷ giá mới nhất so với VND
fetch('https://open.er-api.com/v6/latest/VND')
    .then(response => response.json())
    .then(data => {
        if (data && data.rates) {
            currentRates = data.rates;
            // Sau khi có tỷ giá thực tế, cập nhật lại toàn bộ giao diện một lần nữa
            window.updateAllPrices(currentSelectedCurrency);
        }
    })
    .catch(error => console.error("Dùng tỷ giá dự phòng. Lỗi API:", error));

// Hàm định dạng hiển thị tiền tệ
window.formatCurrency = function (value, currency) {
    if (currency === "VND") return new Intl.NumberFormat('vi-VN').format(Math.round(value)) + " đ";
    if (currency === "USD") return "$" + new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value);
    if (currency === "EUR") return "€" + new Intl.NumberFormat('de-DE', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value);
    return value; // Mặc định nếu không lọt vào 3 case trên
};

// Hàm cập nhật toàn bộ giá trên trang
window.updateAllPrices = function (currency) {
    currentSelectedCurrency = currency;

    // Quét tất cả các thẻ có thuộc tính data-base-price
    document.querySelectorAll('[data-base-price]').forEach(priceElement => {
        const basePrice = parseFloat(priceElement.getAttribute('data-base-price'));
        if (!isNaN(basePrice)) {
            const convertedPrice = basePrice * currentRates[currency];
            priceElement.innerHTML = window.formatCurrency(convertedPrice, currency);
        }
    });

    // Kích hoạt Event tùy chỉnh (Để các trang khác có thể bắt sự kiện và tính toán thêm nếu cần)
    document.dispatchEvent(new CustomEvent('currencyChanged', { detail: { currency: currency } }));
};

// Lắng nghe sự kiện click từ các nút chọn tiền tệ (Dropdown)
document.addEventListener('click', (e) => {
    // Nếu click vào thẻ <a> hoặc <button> có class currency-option
    const targetBtn = e.target.closest('.currency-option');
    if (targetBtn) {
        e.preventDefault(); // Tránh reload trang nếu là thẻ <a>
        const targetCurrency = targetBtn.getAttribute('data-currency');
        window.updateAllPrices(targetCurrency);
    }
});