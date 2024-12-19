import "dotenv/config";
import DataMapper from "./data-mapper.js";
import ExportContentfulData from "./exporter.js";
import { writeFileSync } from "fs";
import { slugify } from "./helpers/slugify.js";

const getDataMapper = async () => {
    const contentfulData = await ExportContentfulData({});

    const dataMapper = new DataMapper(contentfulData);

    return dataMapper;
};

const createRowsFromSections = (sections) => {
    const getParentQuestionForAnswer = (section, answer) =>
        section.questions.find((q) => q.answers.some((a) => a.id == answer.id));

    const getRowValuesForAnswer = (section, answer, chunk, link) => {
        const matchingQuestion = getParentQuestionForAnswer(section, answer);

        if (!matchingQuestion) {
            return;
        }

        return [
            section.name,
            matchingQuestion.text,
            answer.text,
            chunk.header,
            link,
        ];
    };

    const createLinkForRecommendation = (section) =>
        `https://staging.plan-technology-for-your-school.education.gov.uk/${slugify(
            section.name
        )}/recommendation/preview/${section.recommendation.intros[0].maturity.toLowerCase()}`;

    return sections.flatMap((section) => {
        const link = createLinkForRecommendation(section);

        const createRow = ({ answer, chunk }) =>
            getRowValuesForAnswer(section, answer, chunk, link);

        return section.recommendation.section.chunks
            .flatMap((chunk) =>
                chunk.answers.map((answer) => ({
                    answer,
                    chunk,
                }))
            )
            .map(createRow);
    });
};

const Csv = () => {
    const csvHeaders = [
        "Topic",
        "Question",
        "Answer",
        "Outcome",
        "Staging (Preview) URL",
    ];

    const joinRow = (columns) => columns.join("\t");

    const create = (rows) => [[joinRow(csvHeaders)], ...rows].join("\n");

    return {
        joinRow,
        create,
    };
};

const main = async () => {
    const dataMapper = await getDataMapper();
    const csv = Csv();

    const rows = createRowsFromSections(dataMapper.mappedSections).map(
        csv.joinRow
    );

    const output = csv.create(rows);

    writeFileSync("csv-test.csv", output);
};

main()
    .then(() => console.log("Done"))
    .catch(console.error);
