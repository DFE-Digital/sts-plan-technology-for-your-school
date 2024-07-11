export const getSubmissionTimeText = (time) => {
    const hours = time.getHours() < 13 ? time.getHours() : time.getHours() - 12;
    const minutes = time.getMinutes().toString().length === 1 ? `0${time.getMinutes()}` : `${time.getMinutes()}`;

    return `last completed ${hours}:${minutes}${time.getHours() < 12 ? "am" : "pm"}`
}

export const getSlowTestSubmissionTimeText = (time) => {

    time.setMinutes(time.getMinutes() + 1)    
    const hours = time.getHours() <= 12 ? time.getHours() : time.getHours() - 12;
    const minutes = time.getMinutes() < 10 ? `0${time.getMinutes()}` : time.getMinutes();

    return `last completed ${hours}:${minutes}${time.getHours() < 1 ? "am" : "pm"}`
}