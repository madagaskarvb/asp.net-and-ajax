// ================================================================
// product-modal.js — модалка товара с Promise.all
// ================================================================

document.addEventListener('DOMContentLoaded', function () {

    const grid = document.getElementById('catalogGrid');
    const modalEl = document.getElementById('productModal');
    const modalBody = document.getElementById('modalBody');
    const modalTitle = document.getElementById('productModalLabel');

    if (!grid || !modalEl || !modalBody) return;

    // ============================================================
    // Event delegation: клик по любой карточке (в т.ч. добавленной AJAX-ом)
    // ============================================================
    grid.addEventListener('click', function (e) {
        // Игнорируем клик по кнопке «В корзину» и её дочерним элементам
        if (e.target.closest('.add-to-cart')) return;

        // Ищем ближайшую карточку товара
        const card = e.target.closest('.js-product-card');
        if (!card) return;

        const productId = card.dataset.productId;
        if (!productId) return;

        openProductModal(productId);
    });

    // Поддержка Enter/Space для доступности
    grid.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter' && e.key !== ' ') return;
        const card = e.target.closest('.js-product-card');
        if (!card) return;
        e.preventDefault();
        openProductModal(card.dataset.productId);
    });

    // ============================================================
    // Открытие модалки + параллельная загрузка
    // ============================================================
    async function openProductModal(productId) {
        // 1. Показываем модалку со спиннером
        modalTitle.textContent = 'Загрузка...';
        modalBody.innerHTML =
            '<div class="text-center py-5">' +
            '  <div class="spinner-border text-primary" role="status"></div>' +
            '  <div class="text-muted mt-2">Загружаем данные...</div>' +
            '</div>';

        const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        modal.show();

        try {
            // 2. Два запроса ПАРАЛЛЕЛЬНО через Promise.all
            const [detailsResp, reviewsResp] = await Promise.all([
                fetch('/Catalog?handler=GetProductDetails&id=' + encodeURIComponent(productId)),
                fetch('/Catalog?handler=GetProductReviews&id=' + encodeURIComponent(productId))
            ]);

            // 3. Детали — обязательны
            if (!detailsResp.ok) {
                throw new Error('Details HTTP ' + detailsResp.status);
            }
            const product = await detailsResp.json();

            // 4. Отзывы — опциональны (если не пришли — показываем без них)
            let reviews = [];
            try {
                if (reviewsResp.ok) {
                    reviews = await reviewsResp.json();
                }
            } catch (e) {
                console.warn('Отзывы не загрузились, продолжаем без них', e);
            }

            // 5. Собираем HTML вручную
            modalTitle.textContent = product.name;
            modalBody.innerHTML = buildProductHtml(product, reviews);

        } catch (error) {
            console.error('openProductModal error:', error);
            modalTitle.textContent = 'Ошибка';
            modalBody.innerHTML =
                '<div class="alert alert-danger mb-0">' +
                '  Не удалось загрузить данные о товаре.' +
                '</div>';
        }
    }

    // ============================================================
    // Сборка HTML из JSON
    // ============================================================
    function buildProductHtml(product, reviews) {
        let html = '';

        // --- Картинка + основная информация ---
        html += '<div class="row g-3">';

        html += '<div class="col-md-5">';
        html += '  <img src="' + escapeAttr(product.imageUrl) + '"' +
            '       class="img-fluid rounded" alt="' + escapeAttr(product.name) + '">';
        html += '</div>';

        html += '<div class="col-md-7">';
        html += '  <h4 class="mb-2">' + escapeHtml(product.name) + '</h4>';

        // Цена + скидка
        html += '  <div class="mb-3">';
        html += '    <span class="fs-3 fw-bold text-primary">' + escapeHtml(product.price) + '</span>';
        if (product.discountPercent > 0) {
            html += '    <span class="badge bg-success ms-2">−' + product.discountPercent + '%</span>';
        }
        html += '  </div>';

        // Наличие
        if (product.stock === 0) {
            html += '  <span class="badge bg-danger mb-2">Нет в наличии</span>';
        } else {
            html += '  <span class="badge bg-secondary mb-2">В наличии: ' + product.stock + ' шт.</span>';
        }
        if (product.isHit) {
            html += '  <span class="badge bg-warning text-dark mb-2">🔥 ХИТ</span>';
        }

        // Описание
        html += '  <p class="text-muted mt-2">' + escapeHtml(product.description) + '</p>';

        // Кнопка «В корзину»
        if (product.stock > 0) {
            html += '  <button type="button"' +
                '          class="btn btn-primary add-to-cart"' +
                '          data-product-id="' + product.id + '">' +
                '    🛒 В корзину' +
                '  </button>';
        }

        html += '</div>';   // col-md-7
        html += '</div>';   // row

        // --- Отзывы ---
        html += '<hr class="my-4">';
        html += '<h5 class="mb-3">Отзывы' +
            (reviews.length ? ' <span class="badge bg-secondary">' + reviews.length + '</span>' : '') +
            '</h5>';

        if (reviews.length > 0) {
            html += '<ul class="list-group">';
            reviews.forEach(function (r) {
                html += '<li class="list-group-item">';
                html += '  <div class="d-flex justify-content-between align-items-center mb-1">';
                html += '    <strong>' + escapeHtml(r.author) + '</strong>';
                html += '    <span class="text-warning">' + stars(r.rating) + '</span>';
                html += '  </div>';
                html += '  <div class="text-muted small">' + escapeHtml(r.text) + '</div>';
                html += '</li>';
            });
            html += '</ul>';
        } else {
            html += '<p class="text-muted mb-0">Отзывов пока нет</p>';
        }

        return html;
    }

    // ---- Хелпер: звёздочки рейтинга ----
    function stars(rating) {
        let s = '';
        for (let i = 1; i <= 5; i++) {
            s += i <= rating ? '★' : '☆';
        }
        return s + ' (' + rating + '/5)';
    }

    // ---- Хелпер: экранирование HTML ----
    function escapeHtml(str) {
        if (str == null) return '';
        const div = document.createElement('div');
        div.textContent = String(str);
        return div.innerHTML;
    }

    // ---- Хелпер: экранирование для значения атрибута ----
    function escapeAttr(str) {
        return escapeHtml(str).replace(/"/g, '&quot;');
    }

    // ---- Экспорт для других модулей (необязательно) ----
    window.openProductModal = openProductModal;
});