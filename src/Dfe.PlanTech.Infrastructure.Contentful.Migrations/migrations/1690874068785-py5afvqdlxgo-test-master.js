function migrationFunction(migration, context) {
    const header = migration.createContentType("header");
    header
        .displayField("internalName")
        .name("⚙️ Component - Header")
        .description("")

    const headerInternalName = header.createField("internalName");
    headerInternalName
        .name("Internal Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const headerText = header.createField("text");
    headerText
        .name("Text")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const headerTag = header.createField("tag");
    headerTag
        .name("Tag")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([{ "in": ["H1", "H2", "H3"] }])
        .disabled(false)
        .omitted(false)

    const headerSize = header.createField("size");
    headerSize
        .name("Size")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([{ "in": ["Small", "Medium", "Large", "ExtraLarge"] }])
        .disabled(false)
        .omitted(false)
    header.changeFieldControl("internalName", "builtin", "singleLine")
    header.changeFieldControl("text", "builtin", "singleLine")
    header.changeFieldControl("tag", "builtin", "dropdown")
    header.changeFieldControl("size", "builtin", "dropdown")

    const textBody = migration.createContentType("textBody");
    textBody
        .displayField("internalName")
        .name("⚙️ Component - Text Body")
        .description("")

    const textBodyInternalName = textBody.createField("internalName");
    textBodyInternalName
        .name("Internal Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const textBodyRichText = textBody.createField("richText");
    textBodyRichText
        .name("Rich Text")
        .type("RichText")
        .localized(false)
        .required(true)
        .validations([{ "enabledMarks": ["bold"], "message": "Only bold marks are allowed" }, { "enabledNodeTypes": ["ordered-list", "unordered-list", "hyperlink"], "message": "Only ordered list, unordered list, and link to Url nodes are allowed" }, { "nodes": {} }])
        .disabled(false)
        .omitted(false)
    textBody.changeFieldControl("internalName", "builtin", "singleLine")
    textBody.changeFieldControl("richText", "builtin", "richTextEditor")

    const title = migration.createContentType("title");
    title
        .displayField("internalName")
        .name("⚙️ Component - Title")
        .description("")

    const titleInternalName = title.createField("internalName");
    titleInternalName
        .name("Internal Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const titleText = title.createField("text");
    titleText
        .name("Text")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)
    title.changeFieldControl("internalName", "builtin", "singleLine")
    title.changeFieldControl("text", "builtin", "singleLine")

    const category = migration.createContentType("category");
    category
        .displayField("internalName")
        .name("❓Questionnaire - Category")
        .description("")

    const categoryInternalName = category.createField("internalName");
    categoryInternalName
        .name("Internal Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const categoryHeader = category.createField("header");
    categoryHeader
        .name("Header")
        .type("Link")
        .localized(false)
        .required(true)
        .validations([{ "linkContentType": ["header"] }])
        .disabled(false)
        .omitted(false)
        .linkType("Entry")

    const categoryContent = category.createField("content");
    categoryContent
        .name("Content")
        .type("Array")
        .localized(false)
        .required(false)
        .validations([])
        .disabled(false)
        .omitted(false)
        .items({ "type": "Link", "validations": [{ "linkContentType": ["textBody"] }], "linkType": "Entry" })

    const categorySections = category.createField("sections");
    categorySections
        .name("Sections")
        .type("Array")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)
        .items({ "type": "Link", "validations": [{ "linkContentType": ["section"] }], "linkType": "Entry" })
    category.changeFieldControl("internalName", "builtin", "singleLine")
    category.changeFieldControl("header", "builtin", "entryLinkEditor")
    category.changeFieldControl("content", "builtin", "entryLinksEditor")
    category.changeFieldControl("sections", "builtin", "entryLinksEditor")

    const componentDropDown = migration.createContentType("componentDropDown");
    componentDropDown
        .displayField("internalName")
        .name("Component - Drop Down")
        .description("Drop down compontent.")

    const componentDropDownInternalName = componentDropDown.createField("internalName");
    componentDropDownInternalName
        .name("Internal name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const componentDropDownTitle = componentDropDown.createField("title");
    componentDropDownTitle
        .name("Title")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const componentDropDownContent = componentDropDown.createField("content");
    componentDropDownContent
        .name("Content")
        .type("RichText")
        .localized(false)
        .required(true)
        .validations([{ "enabledMarks": ["bold"], "message": "Only bold marks are allowed" }, { "enabledNodeTypes": ["unordered-list", "ordered-list"], "message": "Only unordered list and ordered list nodes are allowed" }, { "nodes": {} }])
        .disabled(false)
        .omitted(false)
    componentDropDown.changeFieldControl("internalName", "builtin", "singleLine")
    componentDropDown.changeFieldControl("title", "builtin", "richTextEditor")
    componentDropDown.changeFieldControl("content", "builtin", "singleLine")

    const button = migration.createContentType("button");
    button
        .displayField("internalName")
        .name("⚙️ Component - Button")
        .description("")

    const buttonInternalName = button.createField("internalName");
    buttonInternalName
        .name("Internal Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const buttonValue = button.createField("value");
    buttonValue
        .name("Value")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const buttonIsStartButton = button.createField("isStartButton");
    buttonIsStartButton
        .name("Is Start Button?")
        .type("Boolean")
        .localized(false)
        .required(false)
        .validations([])
        .disabled(false)
        .omitted(false)
    button.changeFieldControl("internalName", "builtin", "singleLine")
    button.changeFieldControl("value", "builtin", "singleLine")
    button.changeFieldControl("isStartButton", "builtin", "boolean")

    const buttonWithLink = migration.createContentType("buttonWithLink");
    buttonWithLink
        .displayField("internalName")
        .name("⚙️ Component - Button With Link")
        .description("")

    const buttonWithLinkInternalName = buttonWithLink.createField("internalName");
    buttonWithLinkInternalName
        .name("Internal Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const buttonWithLinkButton = buttonWithLink.createField("button");
    buttonWithLinkButton
        .name("Button")
        .type("Link")
        .localized(false)
        .required(true)
        .validations([{ "linkContentType": ["button"] }])
        .disabled(false)
        .omitted(false)
        .linkType("Entry")

    const buttonWithLinkHref = buttonWithLink.createField("href");
    buttonWithLinkHref
        .name("Href")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)
    buttonWithLink.changeFieldControl("internalName", "builtin", "urlEditor")
    buttonWithLink.changeFieldControl("button", "builtin", "singleLine")
    buttonWithLink.changeFieldControl("href", "builtin", "entryLinkEditor")

    const question = migration.createContentType("question");
    question
        .displayField("internalName")
        .name("❓Questionnaire - Question")
        .description("")

    const questionInternalName = question.createField("internalName");
    questionInternalName
        .name("Internal Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const questionText = question.createField("text");
    questionText
        .name("Text")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const questionHelpText = question.createField("helpText");
    questionHelpText
        .name("Help Text")
        .type("Symbol")
        .localized(false)
        .required(false)
        .validations([])
        .disabled(false)
        .omitted(false)

    const questionAnswers = question.createField("answers");
    questionAnswers
        .name("Answers")
        .type("Array")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)
        .items({ "type": "Link", "validations": [{ "linkContentType": ["answer"] }], "linkType": "Entry" })
    question.changeFieldControl("internalName", "builtin", "singleLine")
    question.changeFieldControl("text", "builtin", "singleLine")
    question.changeFieldControl("helpText", "builtin", "entryLinksEditor")
    question.changeFieldControl("answers", "builtin", "singleLine")

    const answer = migration.createContentType("answer");
    answer
        .displayField("internalName")
        .name("❓Questionnaire - Answer")
        .description("")

    const answerInternalName = answer.createField("internalName");
    answerInternalName
        .name("Internal Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const answerText = answer.createField("text");
    answerText
        .name("Text")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const answerNextQuestion = answer.createField("nextQuestion");
    answerNextQuestion
        .name("Next Question")
        .type("Link")
        .localized(false)
        .required(false)
        .validations([{ "linkContentType": ["question"] }])
        .disabled(false)
        .omitted(false)
        .linkType("Entry")

    const answerMaturity = answer.createField("maturity");
    answerMaturity
        .name("Maturity")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([{ "in": ["None", "Low", "Medium", "High"] }])
        .disabled(false)
        .omitted(false)
    answer.changeFieldControl("internalName", "builtin", "singleLine")
    answer.changeFieldControl("text", "builtin", "singleLine")
    answer.changeFieldControl("nextQuestion", "builtin", "entryLinkEditor")
    answer.changeFieldControl("maturity", "builtin", "dropdown")

    const dynamicContent = migration.createContentType("dynamicContent");
    dynamicContent
        .displayField("internalName")
        .name("⚙️ Component - Dynamic Content")
        .description("")

    const dynamicContentInternalName = dynamicContent.createField("internalName");
    dynamicContentInternalName
        .name("Internal Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const dynamicContentDynamicField = dynamicContent.createField("dynamicField");
    dynamicContentDynamicField
        .name("Dynamic Field")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([{ "in": ["Section Title"] }])
        .disabled(false)
        .omitted(false)
    dynamicContent.changeFieldControl("internalName", "builtin", "singleLine")
    dynamicContent.changeFieldControl("dynamicField", "builtin", "singleLine")

    const buttonWithEntryReference = migration.createContentType("buttonWithEntryReference");
    buttonWithEntryReference
        .displayField("internalName")
        .name("⚙️ Component - Button With Entry Reference")
        .description("")

    const buttonWithEntryReferenceInternalName = buttonWithEntryReference.createField("internalName");
    buttonWithEntryReferenceInternalName
        .name("Internal Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const buttonWithEntryReferenceButton = buttonWithEntryReference.createField("button");
    buttonWithEntryReferenceButton
        .name("Button")
        .type("Link")
        .localized(false)
        .required(true)
        .validations([{ "linkContentType": ["button"] }])
        .disabled(false)
        .omitted(false)
        .linkType("Entry")

    const buttonWithEntryReferenceLinkToEntry = buttonWithEntryReference.createField("linkToEntry");
    buttonWithEntryReferenceLinkToEntry
        .name("Link To Entry")
        .type("Link")
        .localized(false)
        .required(true)
        .validations([{ "linkContentType": ["page", "question"] }])
        .disabled(false)
        .omitted(false)
        .linkType("Entry")
    buttonWithEntryReference.changeFieldControl("internalName", "builtin", "singleLine")
    buttonWithEntryReference.changeFieldControl("button", "builtin", "entryLinkEditor")
    buttonWithEntryReference.changeFieldControl("linkToEntry", "builtin", "entryLinkEditor")

    const page = migration.createContentType("page");
    page
        .displayField("internalName")
        .name("📃 Page")
        .description("")

    const pageInternalName = page.createField("internalName");
    pageInternalName
        .name("InternalName")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const pageSlug = page.createField("slug");
    pageSlug
        .name("Slug")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const pageDisplayBackButton = page.createField("displayBackButton");
    pageDisplayBackButton
        .name("Display back button")
        .type("Boolean")
        .localized(false)
        .required(false)
        .validations([])
        .defaultValue({ "en-US": true })
        .disabled(false)
        .omitted(false)

    const pageDisplayTopicTitle = page.createField("displayTopicTitle");
    pageDisplayTopicTitle
        .name("Display topic title?")
        .type("Boolean")
        .localized(false)
        .required(false)
        .validations([])
        .defaultValue({ "en-US": false })
        .disabled(false)
        .omitted(false)

    const pageTitle = page.createField("title");
    pageTitle
        .name("Title")
        .type("Link")
        .localized(false)
        .required(false)
        .validations([{ "linkContentType": ["title"] }])
        .disabled(false)
        .omitted(false)
        .linkType("Entry")

    const pageContent = page.createField("content");
    pageContent
        .name("Content")
        .type("Array")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)
        .items({ "type": "Link", "validations": [{ "linkContentType": ["textBody", "header", "category", "buttonWithLink", "componentDropDown", "dynamicContent", "buttonWithEntryReference", "insetText"] }], "linkType": "Entry" })
    page.changeFieldControl("internalName", "builtin", "entryLinkEditor")
    page.changeFieldControl("slug", "builtin", "boolean")
    page.changeFieldControl("displayBackButton", "builtin", "singleLine")
    page.changeFieldControl("displayTopicTitle", "builtin", "entryLinksEditor")
    page.changeFieldControl("title", "builtin", "boolean")
    page.changeFieldControl("content", "builtin", "urlEditor")

    const insetText = migration.createContentType("insetText");
    insetText
        .displayField("internalName")
        .name("⚙️ Component - Inset Text")
        .description("")

    const insetTextInternalName = insetText.createField("internalName");
    insetTextInternalName
        .name("Internal Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const insetTextText = insetText.createField("text");
    insetTextText
        .name("Text")
        .type("Text")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)
    insetText.changeFieldControl("internalName", "builtin", "singleLine")
    insetText.changeFieldControl("text", "builtin", "multipleLine")

    const componentTextBodyWithMaturity = migration.createContentType("componentTextBodyWithMaturity");
    componentTextBodyWithMaturity
        .displayField("internalName")
        .name("⚙️ Component - Text Body With Maturity")
        .description("")

    const componentTextBodyWithMaturityInternalName = componentTextBodyWithMaturity.createField("internalName");
    componentTextBodyWithMaturityInternalName
        .name("Internal Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const componentTextBodyWithMaturityTextBody = componentTextBodyWithMaturity.createField("textBody");
    componentTextBodyWithMaturityTextBody
        .name("Text Body")
        .type("Link")
        .localized(false)
        .required(true)
        .validations([{ "linkContentType": ["textBody"] }])
        .disabled(false)
        .omitted(false)
        .linkType("Entry")

    const componentTextBodyWithMaturityMaturity = componentTextBodyWithMaturity.createField("maturity");
    componentTextBodyWithMaturityMaturity
        .name("Maturity")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([{ "in": ["None", "Low", "Medium", "High"] }])
        .disabled(false)
        .omitted(false)
    componentTextBodyWithMaturity.changeFieldControl("internalName", "builtin", "singleLine")
    componentTextBodyWithMaturity.changeFieldControl("textBody", "builtin", "entryLinkEditor")
    componentTextBodyWithMaturity.changeFieldControl("maturity", "builtin", "dropdown")

    const pageRecommendation = migration.createContentType("pageRecommendation");
    pageRecommendation
        .displayField("internalName")
        .name("📃 Page - Recommendation")
        .description("")

    const pageRecommendationInternalName = pageRecommendation.createField("internalName");
    pageRecommendationInternalName
        .name("Internal Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const pageRecommendationTitle = pageRecommendation.createField("title");
    pageRecommendationTitle
        .name("Title")
        .type("Link")
        .localized(false)
        .required(true)
        .validations([{ "linkContentType": ["title"] }])
        .disabled(false)
        .omitted(false)
        .linkType("Entry")

    const pageRecommendationInsetText = pageRecommendation.createField("insetText");
    pageRecommendationInsetText
        .name("Inset Text")
        .type("Link")
        .localized(false)
        .required(true)
        .validations([{ "linkContentType": ["insetText"] }])
        .disabled(false)
        .omitted(false)
        .linkType("Entry")

    const pageRecommendationTextBody = pageRecommendation.createField("textBody");
    pageRecommendationTextBody
        .name("Text Body")
        .type("Link")
        .localized(false)
        .required(true)
        .validations([{ "linkContentType": ["textBody"] }])
        .disabled(false)
        .omitted(false)
        .linkType("Entry")

    const pageRecommendationHeader = pageRecommendation.createField("header");
    pageRecommendationHeader
        .name("Header")
        .type("Array")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)
        .items({ "type": "Link", "validations": [{ "linkContentType": ["header"] }], "linkType": "Entry" })

    const pageRecommendationTextBodyWithMaturity = pageRecommendation.createField("textBodyWithMaturity");
    pageRecommendationTextBodyWithMaturity
        .name("Text Body With Maturity")
        .type("Array")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)
        .items({ "type": "Link", "validations": [{ "linkContentType": ["componentTextBodyWithMaturity"] }], "linkType": "Entry" })

    const pageRecommendationContent = pageRecommendation.createField("content");
    pageRecommendationContent
        .name("Content")
        .type("Array")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)
        .items({ "type": "Link", "validations": [{ "linkContentType": ["header", "buttonWithLink", "componentTextBodyWithMaturity"] }], "linkType": "Entry" })
    pageRecommendation.changeFieldControl("internalName", "builtin", "entryLinkEditor")
    pageRecommendation.changeFieldControl("title", "builtin", "entryLinkEditor")
    pageRecommendation.changeFieldControl("insetText", "builtin", "entryLinkEditor")
    pageRecommendation.changeFieldControl("textBody", "builtin", "entryLinksEditor")
    pageRecommendation.changeFieldControl("header", "builtin", "singleLine")
    pageRecommendation.changeFieldControl("textBodyWithMaturity", "builtin", "entryLinksEditor")
    pageRecommendation.changeFieldControl("content", "builtin", "entryLinksEditor")

    const recommendationPage = migration.createContentType("recommendationPage");
    recommendationPage
        .displayField("internalName")
        .name("📃 Recommendation Page")
        .description("")

    const recommendationPageInternalName = recommendationPage.createField("internalName");
    recommendationPageInternalName
        .name("Internal Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const recommendationPageDisplayName = recommendationPage.createField("displayName");
    recommendationPageDisplayName
        .name("Display Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const recommendationPageMaturity = recommendationPage.createField("maturity");
    recommendationPageMaturity
        .name("Maturity")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([{ "in": ["Low", "Medium", "High"] }])
        .disabled(false)
        .omitted(false)

    const recommendationPagePage = recommendationPage.createField("page");
    recommendationPagePage
        .name("Page")
        .type("Link")
        .localized(false)
        .required(true)
        .validations([{ "linkContentType": ["page"] }])
        .disabled(false)
        .omitted(false)
        .linkType("Entry")
    recommendationPage.changeFieldControl("internalName", "builtin", "singleLine")
    recommendationPage.changeFieldControl("displayName", "builtin", "singleLine")
    recommendationPage.changeFieldControl("maturity", "builtin", "radio")
    recommendationPage.changeFieldControl("page", "builtin", "entryLinkEditor")

    const section = migration.createContentType("section");
    section
        .displayField("internalName")
        .name("❓Questionnaire - Section")
        .description("")

    const sectionInternalName = section.createField("internalName");
    sectionInternalName
        .name("Internal Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const sectionName = section.createField("name");
    sectionName
        .name("Name")
        .type("Symbol")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)

    const sectionInterstitialPage = section.createField("interstitialPage");
    sectionInterstitialPage
        .name("Interstitial Page")
        .type("Link")
        .localized(false)
        .required(true)
        .validations([{ "linkContentType": ["page"] }])
        .disabled(false)
        .omitted(false)
        .linkType("Entry")

    const sectionQuestions = section.createField("questions");
    sectionQuestions
        .name("Questions")
        .type("Array")
        .localized(false)
        .required(true)
        .validations([])
        .disabled(false)
        .omitted(false)
        .items({ "type": "Link", "validations": [{ "linkContentType": ["question"] }], "linkType": "Entry" })

    const sectionRecommendations = section.createField("recommendations");
    sectionRecommendations
        .name("Recommendations")
        .type("Array")
        .localized(false)
        .required(true)
        .validations([{ "size": { "min": 3, "max": 3 } }])
        .disabled(false)
        .omitted(false)
        .items({ "type": "Link", "validations": [{ "linkContentType": ["recommendationPage"] }], "linkType": "Entry" })
    section.changeFieldControl("internalName", "builtin", "singleLine")
    section.changeFieldControl("name", "builtin", "singleLine")
    section.changeFieldControl("interstitialPage", "builtin", "entryCardEditor")
    section.changeFieldControl("questions", "builtin", "entryLinksEditor")
    section.changeFieldControl("recommendations", "builtin", "entryCardsEditor")
    migration.deleteContentType("questionPageBroadbandConnection")
    migration.deleteContentType("taskList")
    migration.deleteContentType("startPage")
    migration.deleteContentType("selfAssessmentLandingPage")
    migration.deleteContentType("landingPage")
    migration.deleteContentType("expandableSection")
    migration.deleteContentType("richTextWithHeader")
    migration.deleteContentType("richTextComponent")
}
module.exports = migrationFunction;
