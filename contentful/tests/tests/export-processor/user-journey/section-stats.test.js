import { SectionStats } from "../../../../export-processor/user-journey/section-stats.js";

describe('SectionStats', () => {
    describe('pathsPerMaturity', () => {
        it('should count paths by maturity level', () => {
            const paths = [
                { maturity: 'basic' },
                { maturity: 'basic' },
                { maturity: 'intermediate' },
                { maturity: 'advanced' }
            ];

            const stats = new SectionStats({ paths });
            
            expect(stats.pathsPerMaturity).toEqual({
                basic: 2,
                intermediate: 1,
                advanced: 1
            });
        });

        it('should return cached result on subsequent calls', () => {
            const paths = [{ maturity: 'basic' }];
            const stats = new SectionStats({ paths });
            
            const result1 = stats.pathsPerMaturity;
            const result2 = stats.pathsPerMaturity;
            
            expect(result1).toBe(result2);
        });

        it('should handle empty paths array', () => {
            const stats = new SectionStats({ paths: [] });
            
            expect(stats.pathsPerMaturity).toEqual({});
        });
    });

    describe('_addPathToMaturityCount', () => {
        it('should increment existing maturity count', () => {
            const stats = new SectionStats({ paths: [] });
            const count = { basic: 1 };
            const path = { maturity: 'basic' };

            const result = stats._addUserJourneyToMaturityCount(count, path);

            expect(result).toEqual({ basic: 2 });
        });

        it('should add new maturity level with count 1', () => {
            const stats = new SectionStats({ paths: [] });
            const count = {};
            const path = { maturity: 'basic' };

            const result = stats._addUserJourneyToMaturityCount(count, path);

            expect(result).toEqual({ basic: 1 });
        });
    });
});