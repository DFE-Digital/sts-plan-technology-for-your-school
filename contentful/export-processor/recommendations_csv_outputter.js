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

    const getRowValuesForAnswer = (section, answer, chunk) => {
        const matchingQuestion = getParentQuestionForAnswer(section, answer);

        const createLinkForRecommendation = (section) =>
        `https://staging.plan-technology-for-your-school.education.gov.uk/${slugify(
            section.name
        )}/recommendation/preview/${section.recommendation.intros[0].maturity.toLowerCase()}#${chunk.header.toLowerCase().replace(/ /g, '-')}`;

        if (!matchingQuestion) {
            return;
        }

        const link = createLinkForRecommendation(section);
        const hyperlink = `=HYPERLINK(""${link}"",""Preview Link"")`;

        return [
            section.name,
            matchingQuestion.text,
            answer.text,
            chunk.header,
            hyperlink,
        ];
    };

        return sections.flatMap((section) => {
            if (!section || !section.name || !section.recommendation?.section?.chunks) {
                return []; 
            }    
    
        const createRow = ({ answer, chunk }) =>
            getRowValuesForAnswer(section, answer, chunk);

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

    const joinRow = (columns) => columns.map(value => `"${value}"`).join(",");

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

    const environment = process.env.ENV ?? process.env.ENVIRONMENT ?? "master";
    const now = new Date().toISOString().replaceAll(":", "").replaceAll("-", "").replace("T", "").split(".")[0];
    writeFileSync(`output/recommendations-export-${environment}-${now}.csv`, output);
};

main()
    .then(() => console.log("Done"))
    .catch(console.error);

export default main;