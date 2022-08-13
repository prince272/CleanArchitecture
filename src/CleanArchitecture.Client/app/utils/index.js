// What is the JavaScript version of sleep()?
// source: https://stackoverflow.com/questions/951021/what-is-the-javascript-version-of-sleep
export const sleep = (ms) => {
    return new Promise(resolve => setTimeout(resolve, ms));
};
