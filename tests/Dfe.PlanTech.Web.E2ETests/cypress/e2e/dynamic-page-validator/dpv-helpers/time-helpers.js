export const getSubmissionTimeText = (time) => {
    const hours = time.getHours() <= 12 ? time.getHours() : time.getHours() - 12;
    const minutes = time.getMinutes() < 10 ? time.getMinutes().padStart(2, "0") : time.getMinutes();

    return `last completed ${hours}:${minutes}${time.getHours() < 12 ? "am" : "pm"}`
}