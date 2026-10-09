import { AnchorScroll } from './anchor-scroll';
import { BrowserHistory } from './browser-history';
import { initAll } from 'govuk-frontend';
import { MultiSelect } from '@ministryofjustice/frontend';

new BrowserHistory();
new AnchorScroll();

initAll();

document
    .querySelectorAll('[data-module="moj-multi-select"]')
    .forEach((element) => {
        new MultiSelect(element);
    });