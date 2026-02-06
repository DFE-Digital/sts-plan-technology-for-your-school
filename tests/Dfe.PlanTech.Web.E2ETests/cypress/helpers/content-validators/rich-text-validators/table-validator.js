/**
 * Function to check if the node type matches the given type.
 * @param {string} type - The type of node to check.
 * @param {object} cell - The cell to check.
 * @returns {boolean} - True if the node type matches the given type, false otherwise.
 */
const isNodeType = (type) => (cell) => cell.nodeType === type;

/**
 * Function to check if all cells in a row have the specified node type.
 * @param {string} type - The type of node to check.
 * @param {array} row - The row to check.
 * @returns {boolean} - True if all cells have the specified node type, false otherwise.
 */
const isRowType = (type) => (row) => row.every(isNodeType(type));

const isTableHeaderRow = isRowType('table-header-cell');
const isTableDataRow = isRowType('table-cell');

//Constants used for finding specific rows in a table
const HEADER_ROW = 'HEADER';
const BODY_ROW = 'BODY';

class TableValidator {
  constructor() {}

  /**
   * Method to validate the table content.
   * @param {object} content - The content to validate.
   */
  validateTable(content) {
    const expectedHeaders = content.content.find(
      (row) => this.getRowType(row.content) == HEADER_ROW,
    );

    if (!expectedHeaders) {
      throw new Error("Couldn't find headers");
    }

    const expectedRows = Array.from(
      content.content.filter((row) => this.getRowType(row.content) == BODY_ROW),
    );

    cy.get('table').then(($table) => {
      const mapped = $table.map((i, table) => {
        const jqueryTable = Cypress.$(table);
        const headerRow = jqueryTable.find('thead tr');
        const bodyRows = jqueryTable.find('tbody tr');

        return {
          header: headerRow,
          body: bodyRows,
        };
      });

      const matchingTableFound = this.anyTableMatches(mapped, expectedHeaders, expectedRows);

      expect(matchingTableFound).to.be.true;
    });
  }

  /**
   * Checks if any table matches the expected headers and rows.
   *
   * @param {Array} tables - The mapped tables
   * @param {Array} expectedHeaders - The expected headers
   * @param {Array} expectedRows - The expected rows
   * @return {boolean} Whether any table matches the expected headers and rows
   */
  anyTableMatches(tables, expectedHeaders, expectedRows) {
    for (const table of tables) {
      const { header, body } = table;
      const headerCells = Cypress.$(header).children();

      if (!this.rowMatches(expectedHeaders.content, headerCells)) {
        continue;
      }

      const matches = this.tableBodyMatches(body, expectedRows);

      if (matches) {
        return true;
      }
    }
    return false;
  }

  /**
   * Check if the table body matches the expected rows.
   *
   * @param {Array} body - the table body to compare
   * @param {Array} expectedRows - the expected rows to match against
   * @return {Boolean} true if the table body matches the expected rows, false otherwise
   */
  tableBodyMatches(body, expectedRows) {
    for (let rowIndex = 0; rowIndex < body.length; rowIndex++) {
      const rowCells = Cypress.$(body[rowIndex]).children();
      const expectedCells = expectedRows[rowIndex].content;

      if (!this.rowMatches(expectedCells, rowCells)) {
        return false;
      }
    }

    return true;
  }

  /**
   * Checks if the cells in a row match the expected cells.
   *
   * @param {array} expectedCells - the array of expected cells
   * @param {array} rowCells - the array of cells in the row
   * @return {boolean} true if all row cells match the expected cells, false otherwise
   */
  rowMatches(expectedCells, rowCells) {
    for (let index = 0; index < expectedCells.length; index++) {
      const expectedCellText = this.getCellText(expectedCells[index]);
      const actualCellText = Cypress.$(rowCells[index]).text();

      const matches = actualCellText == expectedCellText;

      if (!matches) {
        return false;
      }
    }

    return true;
  }

  /**
   * Method to determine the type of a row based on its cells.
   * @param {array} rowCells - The cells of the row.
   * @returns {string} - The type of the row.
   * @throws {Error} - If the row type is invalid.
   */
  getRowType(rowCells) {
    if (isTableHeaderRow(rowCells)) return HEADER_ROW;
    if (isTableDataRow(rowCells)) return BODY_ROW;

    throw new Error('Invalid row type', rowCells);
  }

  /**
   * Method to get the text content of a cell.
   * @param {object} cell - The cell to get the text from.
   * @returns {string} - The text content of the cell.
   */
  getCellText(cell) {
    let text = '';

    function traverseCellAndAppendTextContent(cell) {
      if (cell.value) {
        //Strip out HTML tags due to unescaped characters in certain content
        //E.g. cookies -> analytical cookies
        text += cell.value.replace(/(<([^>]+)>)/gi, '');
      }

      if (cell.content) {
        for (const child of cell.content) {
          traverseCellAndAppendTextContent(child);
        }
      }
    }

    traverseCellAndAppendTextContent(cell);
    return text;
  }
}

export default TableValidator;
