/**
 * Logs specific accessibilty violations to console as table
 * Stolen from their documentation: https://github.com/component-driven/cypress-axe
 * @param {*} violations Violated accessibility rules
 */
export function terminalLog(violations) {
  cy.task(
    'log',
    `${violations.length} accessibility violation${
      violations.length === 1 ? '' : 's'
    } ${violations.length === 1 ? 'was' : 'were'} detected`,
  );

  const violationData = violations.map(({ id, impact, description, nodes }) => ({
    id,
    impact,
    description,
    nodes: nodes.length,
  }));

  cy.task('table', violationData);

  // Log full page HTML for debugging
  cy.document().then((doc) => {
    cy.task('log', '\n\n==================== FULL PAGE HTML ====================');
    cy.task('log', doc.documentElement.outerHTML);
    cy.task('log', '==================== END FULL PAGE HTML ====================\n');
  });

  // Log detailed node information for debugging
  violations.forEach((violation) => {
    cy.task('log', `\n=== Violation: ${violation.id} ===`);
    cy.task('log', `Description: ${violation.description}`);
    cy.task('log', `Impact: ${violation.impact}`);
    cy.task('log', `Help: ${violation.help}`);
    cy.task('log', `Help URL: ${violation.helpUrl}`);

    violation.nodes.forEach((node, index) => {
      cy.task('log', `\n--- Node ${index + 1} of ${violation.nodes.length} ---`);
      cy.task('log', `Target: ${node.target.join(' > ')}`);
      cy.task('log', `HTML: ${node.html}`);
      cy.task('log', `Failure summary: ${node.failureSummary}`);
    });
  });
}

/**
 * Wrapper around the cy.checkA11y command (i.e. the command to run axe tests)
 * That adds default ignore rules, and outputs the failed accessibility rules via
 * terminalLog method
 */
Cypress.Commands.add('runAxe', () => {
  cy.checkA11y(
    null,
    {
      rules: {
        //Ignore warning about region
        //Related to back button being outside of main + header tags, but this is based on GDS guidelines
        region: { enabled: false },
      },
    },
    terminalLog,
  );
});
