import AppendixRow from "#src/test-suite/appendix-row";

export default class Appendix {
  rows = [];

  addRow({ reference, content }) {
    this.rows.push(new AppendixRow({ reference, content }));
  }
}
