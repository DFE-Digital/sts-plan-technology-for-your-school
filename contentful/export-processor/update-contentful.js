import pkg from 'contentful-management';
import { options } from "./options.js";

const { createClient, sdk } = pkg;
const cma = createClient(
  {
    ...options,
    accessToken: options.managementToken
  }
);

const updateEditorInterface = async () => {
  const space = await cma.getSpace(options.spaceId);
  const env = await space.getEnvironment(options.environment);
  const content = await env.getEditorInterfaces();

  console.log(content);
};

updateEditorInterface().then(() => console.log(""));