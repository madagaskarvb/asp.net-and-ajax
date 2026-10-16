// ================================================================
// site.js — общие скрипты для всех страниц
// ================================================================

document.addEventListener('DOMContentLoaded', function () {

    // -------- 1. Автоскрытие alert-сообщений с классом .auto-dismiss --------
    document.querySelectorAll('.alert.auto-dismiss').forEach(function (el) {
        setTimeout(function () {
            el.classList.remove('show');
            setTimeout(function () { el.remove(); }, 300);
        }, 3000);
    });

    // -------- 2. Тег-фильтр: мгновенная подсветка при клике --------
    const filters = document.querySelectorAll('.tag-filter');
    filters.forEach(function (btn) {
        btn.addEventListener('click', function () {
            filters.forEach(function (b) { b.classList.remove('active'); });
            this.classList.add('active');
        });
    });

    // -------- 3. Счётчик корзины при загрузке ЛЮБОЙ страницы --------
    loadCartCount();
});

// ================================================================
// Загружаем текущее количество товаров в корзине и обновляем бейдж
// ================================================================
async function loadCartCount() {
    const badge = document.getElementById('cartBadge');
    if (!badge) return;   // на странице нет бейджа (например, отдельный layout) — выходим

    try {
        const response = await fetch('/Catalog?handler=GetCartCount');

        if (!response.ok) {
            throw new Error('HTTP ' + response.status);
        }

        const data = await response.json();

        badge.textContent = data.count;

        // Скрываем бейдж, если корзина пуста — так чище, чем красный "0"
        badge.style.display = data.count > 0 ? '' : 'none';

    } catch (error) {
        console.error('Не удалось получить количество товаров:', error);
        // Тихо игнорируем — не критично для UX
    }
}

// Экспортируем функцию в глобальную область,
// чтобы cart.js мог её вызывать после успешного добавления
window.loadCartCount = loadCartCount;