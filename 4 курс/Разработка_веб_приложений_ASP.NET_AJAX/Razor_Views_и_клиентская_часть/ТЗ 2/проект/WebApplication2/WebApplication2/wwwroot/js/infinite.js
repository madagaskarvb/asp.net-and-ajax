// ================================================================
// Infinite Scroll — автоподгрузка товаров при прокрутке
// ================================================================

// Глобальное состояние пагинации — сюда же будет писать search.js
window.catalogPagination = {
    currentPage: 1,
    isLoading: false,
    hasMore: true,

    // Полный сброс (например, после очистки поиска)
    reset: function () {
        this.currentPage = 1;
        this.isLoading = false;
        this.hasMore = true;
    },

    // Отключить (пока идёт поиск — новый контент уже не «докрутить»)
    disable: function () {
        this.hasMore = false;
    },

    // Включить и синхронизировать номер страницы
    enable: function (page) {
        this.currentPage = page || 1;
        this.isLoading = false;
        this.hasMore = true;
    }
};

document.addEventListener('DOMContentLoaded', function () {

    const grid = document.getElementById('catalogGrid');
    const sentinel = document.getElementById('sentinel');
    const spinner = document.getElementById('loadMoreSpinner');

    // Если мы не на странице каталога — выходим
    if (!grid || !sentinel) return;

    // ============================================================
    // IntersectionObserver: срабатывает, когда sentinel попадает
    // в область видимости (rootMargin расширяет её на 200px)
    // ============================================================
    const observer = new IntersectionObserver(async function (entries) {

        if (!entries[0].isIntersecting) return;

        const state = window.catalogPagination;
        if (state.isLoading || !state.hasMore) return;

        // Блокируем повторные срабатывания
        state.isLoading = true;
        if (spinner) spinner.classList.remove('d-none');

        try {
            // Увеличиваем номер страницы и запрашиваем следующую порцию
            state.currentPage++;
            const url = '/Catalog?handler=LoadMore&page=' + state.currentPage;

            const response = await fetch(url);
            if (!response.ok) throw new Error('HTTP ' + response.status);

            const html = await response.text();

            // Пустой ответ = товары закончились
            if (!html.trim()) {
                state.hasMore = false;
                observer.disconnect();  // больше не наблюдаем за sentinel
                return;
            }

            // Вставляем новую порцию в конец сетки
            grid.insertAdjacentHTML('beforeend', html);

        } catch (error) {
            console.error('LoadMore error:', error);
            state.hasMore = false;   // чтобы не долбить сервер при ошибке
        } finally {
            state.isLoading = false;
            if (spinner) spinner.classList.add('d-none');
        }

    }, { rootMargin: '200px' });  // срабатывает за 200px до конца

    observer.observe(sentinel);

    // Экспортируем observer для search.js (чтобы пере-подключаться)
    window.catalogObserver = observer;
});