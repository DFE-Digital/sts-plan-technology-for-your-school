export default function mapContent(content) {
  if (!content) {
    return [];
  }

  return content.map((item) => {
    const contentType = item.sys.contentType.sys.id;

    switch (contentType) {
      case 'header': {
        return processHeader(item);
      }

      case 'textBody': {
        return processTextBody(item);
      }

      default: {
        console.log('not mapped content type', contentType);
        return '';
      }
    }
  });
}

function processHeader(header) {
  return `Header: ${header.fields.text}`;
}

function processTextBody(textBody) {
  return 'Text: \n' + processRichText(textBody.fields.richText);
}

function processRichText(richText) {
  const values = [];

  if (richText.value) {
    values.push(richText.value);
  }

  for (const value of yieldValues(richText)) {
    values.push(value);
  }

  return values.join('\n');
}

function* yieldValues(richText) {
  if (!richText) {
    return;
  }

  if (richText.value) {
    yield richText.value;
  }

  if (!richText.content) {
    return;
  }

  for (const content of richText.content) {
    for (const innerValue of yieldValues(content)) {
      yield innerValue;
    }
  }
}
