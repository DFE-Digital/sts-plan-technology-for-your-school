import fs from "fs";
import { Section } from "./section.mjs";

const file = "contentful-export-py5afvqdlxgo-dev-2024-01-17T13-18-05.json";
const fileContents = fs.readFileSync(file, "utf-8");
const asJsonObject = JSON.parse(fileContents);

const entries = asJsonObject.entries;

const sections = new Map();
const questions = new Map();
const answers = new Map();
const recommendations = new Map();

for (const entry of entries) {
  const contentType = entry.sys.contentType.sys.id;
  const id = entry.sys.id;

  const mappedFields = {};
  Object.entries(entry.fields).forEach(([key, value]) => {
    mappedFields[key] = stripLocalisation(value);
  });

  entry.fields = mappedFields;

  switch (contentType) {
    case "answer": {
      answers.set(id, entry);
      break;
    }
    case "question": {
      questions.set(id, entry);
      break;
    }
    case "section": {
      sections.set(id, entry);
      break;
    }
    case "recommendationPage": {
      recommendations.set(id, entry);
      break;
    }
  }
}

combineEntries();

const sectionClasses = new Map();

for (const [id, section] of sections) {
  const asClass = new Section(section);
  console.log(asClass);
}

const sectionPaths = new Map();

for (const [id, section] of sections) {
  const firstQuestion = section.fields.questions[0];
  const paths = getAllPaths(section.fields.questions, firstQuestion);

  const result = {
    name: section.fields.name,
    id: section.sys.id,
    paths: paths,
  };

  var stringified = JSON.stringify(result);
  fs.writeFileSync(result.name + ".json", stringified);
}

fs.writeFileSync("paths.json", JSON.stringify(sectionPaths));

function getAllPathsRecursion(questions, paths, currentPath, currentQuestion) {
  if (!currentQuestion) {
    return paths;
  }

  currentQuestion.fields.answers.forEach((answer) => {
    const newPath = [
      ...currentPath,
      { question: currentQuestion.fields.text, answer: answer.fields.text },
    ];

    console.log(newPath);
    const nextQuestion = questions.find(
      (q) => q.sys.id === answer.fields.nextQuestion?.sys.id
    );

    const nextPaths = getAllPaths(questions, paths, newPath, nextQuestion);
    paths.push(...nextPaths);
  });

  return paths.length ? paths : [currentPath];
}

function getAllPaths(questions, currentQuestion) {
  const paths = [];
  const stack = [];

  stack.push({
    currentPath: [],
    currentQuestion,
  });

  while (stack.length > 0) {
    const { currentPath, currentQuestion } = stack.pop();

    if (!currentQuestion) {
      paths.push(currentPath);
      continue;
    }

    currentQuestion.fields.answers.forEach((answer) => {
      const newPath = [
        ...currentPath,
        { question: currentQuestion.fields.text, answer: answer.fields.text },
      ];

      const nextQuestion = questions.find(
        (q) => q.sys.id === answer.fields.nextQuestion?.sys.id
      );

      stack.push({
        currentPath: newPath,
        currentQuestion: nextQuestion,
      });
    });
  }

  return paths;
}

function combineEntries() {
  for (const [id, question] of questions) {
    question.fields.answers = copyRelationships(
      question.fields.answers,
      answers
    );
  }

  for (const [id, section] of sections) {
    section.fields.questions = copyRelationships(
      section.fields.questions,
      questions
    );

    console.log(section.fields.recommendations);
    console.log(recommendations);
    section.fields.recommendations = copyRelationships(
      section.fields.recommendations,
      recommendations
    );
  }
}

function stripLocalisation(obj) {
  return obj["en-US"];
}

function copyRelationships(parent, children) {
  return parent.map((child) => children.get(child.sys.id));
}

function onlyUnique(value, index, array) {
  return array.indexOf(value) === index;
}
