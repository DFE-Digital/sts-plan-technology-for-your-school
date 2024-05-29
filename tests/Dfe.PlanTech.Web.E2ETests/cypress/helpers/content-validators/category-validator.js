import ValidateHeader from "./header-validator.js";
import ValidateSection from "./section-validator.js";

function ValidateCategory({ fields, sys }) {
  const header = fields.header;

  const headerTest = ValidateHeader(header);

  const progressHeaderTest = headerTest.parent().next().contains("Progress");

  const progressTest = progressHeaderTest
    .next("p")
    .contains("You have completed");

  const appTaskList = progressTest
    .next("ul.app-task-list__items")
    .should("exist");

  const sections = fields.sections;

  appTaskList.find("li.app-task-list__item").then(($listItems) => {
    expect($listItems).to.have.length(sections.length);

    for (let index = 0; index < sections.length; index++) {
      const section = sections[index];
      const listItem = $listItems[index];

      ValidateSection(section, listItem);
    }
  });
}

export default ValidateCategory;
