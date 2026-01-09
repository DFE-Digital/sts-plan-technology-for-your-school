import AppendixRow from './appendix-row.js';

export default class Appendix {
  rows = [];

  addRow({ reference, content }) {
    this.rows.push(new AppendixRow({ reference, content }));
  }
}
