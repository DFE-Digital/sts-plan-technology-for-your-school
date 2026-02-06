export const getSubmissionTimeText = (time) => {
  const formatted = time
    .toLocaleTimeString('en-US', {
      timeStyle: 'short',
    })
    .toLowerCase()
    .replace(' ', '');

  return `last completed ${formatted}`;
};
