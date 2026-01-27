/**
 * @typedef {(number | undefined)} MaturityCount
 */

/**
 * @typedef {import("./user-journey").UserJourney} UserJourney
 */

/**
 * @typedef {"Low": MaturityCount, "Medium": MaturityCount, "High": MaturityCount} PathCountPerMaturity
 */

export class SectionStats {
  _pathsPerMaturity;

  /**
   *
   * @param {{paths: UserJourney[]}} params
   */
  constructor({ paths }) {
    this.paths = paths;

    this._addUserJourneyToMaturityCount = this._addUserJourneyToMaturityCount.bind(this);
  }

  /**
   * Calculates the statistics of the paths.
   * Currently just counts the number of paths for each maturity level.
   * @returns {PathCountPerMaturity}
   */
  get pathsPerMaturity() {
    if (this._pathsPerMaturity !== undefined) {
      return this._pathsPerMaturity;
    }

    this._pathsPerMaturity = this.paths.reduce(this._addUserJourneyToMaturityCount, {});
    return this._pathsPerMaturity;
  }

  /**
   * @private
   * @param {number} count
   * @param {UserJourney} userJourney
   * @returns {PathCountPerMaturity}
   */
  _addUserJourneyToMaturityCount(count, userJourney) {
    const maturity = userJourney.maturity;
    return (count[maturity] ? ++count[maturity] : (count[maturity] = 1), count);
  }
}
