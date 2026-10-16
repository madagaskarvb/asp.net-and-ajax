// ================================================================
// AJAX-добавление товаров в корзину + Bootstrap Toast
// ================================================================

document.addEventListener('DOMContentLoaded', function () {

    const grid = document.getElementById('catalogGrid');
    const badge = document.getElementById('cartBadge');

    // Event delegation: один слушатель на контейнер
    if (grid) {
        grid.addEventListener('click', function (e) {
            const btn = e.target.closest('.add-to-cart');
            if (!btn || btn.disabled) return;

            addToCart(btn.dataset.productId, btn);
        });
    }

    // ---------- Показать Bootstrap Toast ----------
    function showCartToast(message, type) {
        const toastEl = document.getElementById('cartToast');
        const toastText = document.getElementById('cartToastText');
        if (!toastEl || !toastText) return;

        // Меняем текст
        toastText.textContent = message;

        // Меняем цвет под тип: success (зелёный) / danger (красный)
        toastEl.classList.remove('bg-success', 'bg-danger');
        toastEl.classList.add(type === 'danger' ? 'bg-danger' : 'bg-success');

        // Создаём и показываем (delay — время до автоскрытия)
        const toast = new bootstrap.Toast(toastEl, { delay: 3000 });
        toast.show();
    }

    // ---------- Главная функция ----------
    async function addToCart(productId, button) {
        const originalText = button.textContent;
        const originalClass = button.className;

        // Показываем спиннер на время запроса
        button.disabled = true;
        button.innerHTML =
            '<span class="spinner-border spinner-border-sm me-1"></span> Добавление...';

        try {
            const token = document
                .querySelector('meta[name="csrf-token"]')
                ?.getAttribute('content') || '';

            const response = await fetch('/Catalog?handler=AddToCart', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': token
                },
                body: 'id=' + encodeURIComponent(productId)
            });

            if (!response.ok) throw new Error('HTTP ' + response.status);

            const data = await response.json();

            if (data.success) {
                // 1. Обновляем бейдж
                if (badge) {
                    badge.textContent = data.cartCount;
                    toggleBadgeVisibility(data.cartCount);
                    pulseBadge();
                }

                // 2. Показываем Bootstrap Toast
                showCartToast('Товар «' + data.productName + '» добавлен в корзину');
            } else {
                showCartToast(data.message || 'Ошибка добавления', 'danger');
            }
        } catch (error) {
            console.error('AddToCart error:', error);
            showCartToast('Не удалось добавить товар. Попробуйте ещё раз.', 'danger');
        } finally {
            // В ЛЮБОМ случае возвращаем кнопку в исходный вид
            button.innerHTML = originalText;
            button.className = originalClass;
            button.disabled = false;
        }
    }

    // ---------- Скрываем бейдж, если корзина пуста ----------
    function toggleBadgeVisibility(count) {
        if (!badge) return;
        badge.style.display = count > 0 ? '' : 'none';
    }

    // ---------- Анимация пульса бейджа ----------
    function pulseBadge() {
        if (!badge) return;
        badge.classList.add('badge-pulse');
        setTimeout(function () {
            badge.classList.remove('badge-pulse');
        }, 600);
    }
});