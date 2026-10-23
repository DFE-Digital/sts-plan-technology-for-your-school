export class AnchorScroll {
    constructor() {
        document.addEventListener('click', (event) => {
            const link = event.target.closest('[data-module="anchor-scroll"]');

            if (!link) {
                return;
            }

            const href = link.getAttribute('href');

            if (!href?.startsWith('#')) {
                return;
            }

            const target = document.querySelector(href);

            if (!target) {
                return;
            }

            event.preventDefault();
            target.scrollIntoView({ block: 'start' });
        });
    }
}