const isNodeType = (type) => (cell) => cell.nodeType === type;

const isRowType = (type) => (row) => row.every(isNodeType(type));

const isTableHeaderRow = isRowType("table-header-cell");
const isTableDataRow = isRowType("table-cell");

function validateTable(content) {
  cy.get("table").then(($table) => {
    let foundTable = null;

    for (const row of content.content) {
      foundTable = validateRow(row, foundTable, $table);
    }
  });
}

function validateRow(row, foundTable, $table) {
  const cells = row.content;

  const rowType = getRowType(cells, row);

  for (var x; x < cells.length; x++) {
    const selector = getSelectorForRow(rowType, x);

    const cell = cells[x];
    const cellText = getCellText(cell);

    const tableToCheck = foundTable ?? cy.wrap($table);

    tableToCheck
      .get(selector)
      .contains(cellText)
      .then((matchingTable) => {
        if (foundTable == null) {
          foundTable = matchingTable;
        }
      });
  }

  return foundTable;
}

function getSelectorForRow(rowType, x) {
  return rowType == "HEADER"
    ? "thead tr th"
    : x == 0
    ? "tbody tr th"
    : "tbody tr td";
}

function getRowType(cells, row) {
  const rowType = isTableHeaderRow(cells)
    ? "HEADER"
    : isTableDataRow(cells)
    ? "DATA"
    : null;

  if (!rowType) throw new Error("Invalid row type", row);
  return rowType;
}

function getCellText(cell) {
  let text = "";

  function recursive(cell) {
    console.log(`checking cell`, cell);
    if (cell.value) {
      text += cell.value;
    }

    if (cell.content) {
      for (const child of cell.content) {
        recursive(child);
      }
    }
  }

  recursive(cell);
  return text;
}

export default validateTable;
