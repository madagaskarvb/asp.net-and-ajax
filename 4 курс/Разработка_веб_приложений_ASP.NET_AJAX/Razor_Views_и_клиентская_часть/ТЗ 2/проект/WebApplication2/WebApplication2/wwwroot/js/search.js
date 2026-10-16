// ================================================================
// search.js — Live-поиск по каталогу через fetch (vanilla JS)
// + синхронизация с бесконечной прокруткой (infinite.js)
// ================================================================

document.addEventListener('DOMContentLoaded', function () {

    const input = document.getElementById('searchInput');
    const grid = document.getElementById('catalogGrid');
    const clearBtn = document.getElementById('searchClear');

    // Если мы не на странице каталога — ничего не делаем
    if (!input || !grid) return;

    let timeoutId = null;

    // ----------------------------------------------------------
    // 1. Обработчик ввода с дебаунсом
    // ----------------------------------------------------------
    input.addEventListener('input', function () {
        clearTimeout(timeoutId);

        const query = input.value.trim();

        // Пустой запрос или 1 символ — показываем все товары
        if (query.length < 2) {
            timeoutId = setTimeout(function () {
                loadAllProducts();
            }, 300);
            return;
        }

        // Иначе — ждём 300 мс и шлём запрос
        timeoutId = setTimeout(function () {
            searchProducts(query);
        }, 300);
    });

    // ----------------------------------------------------------
    // 2. Кнопка «Очистить»
    // ----------------------------------------------------------
    if (clearBtn) {
        clearBtn.addEventListener('click', function () {
            input.value = '';
            clearTimeout(timeoutId);
            loadAllProducts();
            input.focus();
        });
    }

    // ----------------------------------------------------------
    // 3. Загрузка всех товаров (пустой query)
    // ----------------------------------------------------------
    async function loadAllProducts() {
        grid.innerHTML =
            '<div class="text-center py-5">' +
            '  <div class="spinner-border text-primary" role="status"></div>' +
            '</div>';

        try {
            const response = await fetch('/Catalog?handler=Search');
            if (!response.ok) throw new Error('HTTP ' + response.status);

            const html = await response.text();
            grid.innerHTML = html;

            // Сбрасываем пагинацию — снова работает бесконечная прокрутка
            if (window.catalogPagination) {
                window.catalogPagination.reset();
            }

            // Снова подключаем observer, если был отключён
            if (window.catalogObserver && window.catalogPagination?.hasMore) {
                const sentinel = document.getElementById('sentinel');
                if (sentinel) {
                    // observer.unobserve + observe — безопасный "перезапуск"
                    try { window.catalogObserver.observe(sentinel); } catch (e) { /* уже наблюдает */ }
                }
            }

            updateProductCountBadge();

        } catch (error) {
            grid.innerHTML =
                '<div class="alert alert-danger">Ошибка загрузки каталога</div>';
            console.error('loadAllProducts error:', error);
        }
    }

    // ----------------------------------------------------------
    // 4. Поиск
    // ----------------------------------------------------------
    async function searchProducts(query) {
        // Спиннер
        grid.innerHTML =
            '<div class="text-center py-5">' +
            '  <div class="spinner-border text-primary" role="status"></div>' +
            '  <div class="text-muted mt-2">Поиск...</div>' +
            '</div>';

        try {
            const url = '/Catalog?handler=Search&query=' + encodeURIComponent(query);
            const response = await fetch(url);

            if (!response.ok) {
                throw new Error('HTTP ' + response.status);
            }

            const html = await response.text();

            // Пустой результат или явное «Ничего не найдено»
            if (!html.trim() || html.includes('Ничего не найдено')) {
                grid.innerHTML =
                    '<div class="alert alert-info text-center">' +
                    '  Ничего не найдено по запросу «' +
                    escapeHtml(query) +
                    '»' +
                    '</div>';
            } else {
                grid.innerHTML = html;
            }

            // Отключаем бесконечную прокрутку, пока показаны результаты поиска
            if (window.catalogPagination) {
                window.catalogPagination.disable();
            }

            updateProductCountBadge();

        } catch (error) {
            grid.innerHTML =
                '<div class="alert alert-danger">' +
                '  Ошибка поиска. Попробуйте ещё раз.' +
                '</div>';
            console.error('searchProducts error:', error);
        }
    }

    // ----------------------------------------------------------
    // 5. Бейдж «N товаров» над поиском
    // ----------------------------------------------------------
    function updateProductCountBadge() {
        const badge = document.getElementById('product-count-badge');
        if (!badge) return;

        const cards = grid.querySelectorAll('.product-card');
        const n = cards.length;

        let word = 'товаров';
        if (n % 10 === 1 && n % 100 !== 11) word = 'товар';
        else if ([2, 3, 4].includes(n % 10) && ![12, 13, 14].includes(n % 100)) word = 'товара';

        badge.textContent = n + ' ' + word;
    }

    // ----------------------------------------------------------
    // 6. Хелпер: экранирование HTML
    // ----------------------------------------------------------
    function escapeHtml(str) {
        const div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }
});