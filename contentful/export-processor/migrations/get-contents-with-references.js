// eslint-disable-next-line no-unused-vars
import DataMapper from '../data-mapper.js';

/**
 * Creates an object with all content entries with their outgoing references
 * @param {object} params
 * @param {DataMapper} params.dataMapper
 * @param {object[]} params.entries - original exported contentful entries
 */
export const getContentsWithReferences = ({ dataMapper, entries }) => {
  const contents = getOutgoingReferences(dataMapper);

  getIncomingReferences(contents);

  return addOriginalContentEntries({ contents, entries });
};

function getOutgoingReferences(dataMapper) {
  const contents = {};

  for (const [contentTypeName, contentType] of dataMapper.contentTypes.entries()) {
    const referenceFields = Object.entries(contentType.referenceFields);

    const contentsForType = dataMapper.contents.get(contentTypeName);

    if (!contentsForType) {
      console.log(`Found no contents for type ${contentTypeName}`);
      continue;
    }

    for (const [id, content] of contentsForType) {
      contents[id] = getContentsWithReferenceFields({ referenceFields, content, id });
    }
  }

  return contents;
}

function getIncomingReferences(contents) {
  for (const [id, content] of Object.entries(contents)) {
    if (content.referencedIds.length == 0) {
      continue;
    }

    for (const referencedId of content.referencedIds) {
      const matching = contents[referencedId];

      if (!matching) {
        console.error(
          `Content ${id} has outgoing reference ID ${referencedId} which was not found`,
        );
        continue;
      }

      matching.referencedByIds.add(id);
    }
  }
}

function getContentsWithReferenceFields({ referenceFields, content, id }) {
  const referenceFieldValues = referenceFields
    .map((field) => field[0])
    .map((field) => content.fields[field])
    .filter((field) => !!field)
    .flatMap((field) => field);

  return {
    id: id,
    referencedIds: Array.from(referenceFieldValues.map(getReferenceId).filter((id) => !!id)) ?? [],
    referencedByIds: new Set(),
  };
}

/**
 * Adds the original contentful entry value to each content
 * @param {*} param0
 * @returns
 */
function addOriginalContentEntries({ contents, entries }) {
  return Object.entries(contents).map(([id, content]) => {
    const matchingEntry = entries.find((entry) => entry.sys.id == id);

    if (!matchingEntry) {
      console.error(`Couldn't find original entry for ${id}`);
      return null;
    }

    return {
      ...content,
      referencedByIds: Array.from(content.referencedByIds.values()) ?? [],
      entry: matchingEntry,
    };
  });
}
const getReferenceId = (value) => {
  const id = value?.sys?.id;

  if (!id) {
    console.log('couldnt find reference for ' + value);
  }

  return id;
};
