import { BrowserHistory } from './browser-history';
import { initAll } from 'govuk-frontend';
import { MultiSelect } from '@ministryofjustice/frontend';

new BrowserHistory();

initAll();

document
    .querySelectorAll('[data-module="moj-multi-select"]')
    .forEach((element) => {
        new MultiSelect(element);
    });