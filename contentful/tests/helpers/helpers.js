import { faker } from "@faker-js/faker";

const randomRange = ({ min = 1, max = 10, callback }) => {
    const contentCount = faker.number.int({ min, max });
    return Array.from({ length: contentCount }, () => {
        try{
        const result = callback();

        return result;
        }
        catch(err){
            console.error("error running callback", err, callback);
        }
    });
};

export { randomRange };
