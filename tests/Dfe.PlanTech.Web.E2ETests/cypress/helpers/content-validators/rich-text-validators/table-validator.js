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

const isTableHeaderRow = isRowType("table-header-cell");
const isTableDataRow = isRowType("table-cell");

class TableValidator {
  foundTable;

  constructor() {}

  /**
   * Method to validate the table content.
   * @param {object} content - The content to validate.
   */
  validateTable(content) {
    cy.get("table").then(($table) => {
      for (const row of content.content) {
        this.validateRow(row, $table);
      }

      this.foundTable = null;
    });
  }

  /**
   * Validates a row of the table.
   * @param {object} row - The row to validate.
   * @param {object} tableElement - The table element.
   */
  validateRow(row, tableElement) {
    const cells = row.content;

    const rowType = this.getRowType(cells, row);

    for (let index = 0; index < cells.length; index++) {
      const cellSelector = this.getCellSelector(rowType, index);
      const cell = cells[index];
      const tableToCheck = this.foundTable ?? cy.wrap(tableElement);

      this.validateCell(cell, tableToCheck, cellSelector);
    }
  }

  /**
   * Method to validate a cell of the table.
   * @param {object} cell - The cell to validate.
   * @param {object} tableToCheck - The table to check.
   * @param {string} cellSelector - The selector for the cell.
   */
  validateCell(cell, tableToCheck, cellSelector) {
    const cellText = this.getCellText(cell);

    tableToCheck
      .get(cellSelector)
      .contains(cellText)
      .then((matchingTable) => {
        if (this.foundTable == null) {
          this.foundTable = matchingTable;
        }
      });
  }

  /**
   * Method to get the selector for a row based on its type and position.
   * @param {string} rowType - The type of the row.
   * @param {number} index - The position of the cell in the row.
   * @returns {string} - The selector for the row.
   */
  getCellSelector(rowType, index) {
    if (rowType === "HEADER") {
      return "thead tr th";
    } else if (index === 0) {
      return "tbody tr th";
    } else {
      return "tbody tr td";
    }
  }

  /**
   * Method to determine the type of a row based on its cells.
   * @param {array} rowCells - The cells of the row.
   * @returns {string} - The type of the row.
   * @throws {Error} - If the row type is invalid.
   */
  getRowType(rowCells, row) {
    if (isTableHeaderRow(rowCells)) return "HEADER";
    if (isTableDataRow(rowCells)) return "DATA";

    throw new Error("Invalid row type", rowCells);
  }

  /**
   * Method to get the text content of a cell.
   * @param {object} cell - The cell to get the text from.
   * @returns {string} - The text content of the cell.
   */
  getCellText(cell) {
    let text = "";

    function traverseCellAndAppendTextContent(cell) {
      if (cell.value) {
        text += cell.value;
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
