
export function validatePhone(phone: string) {
    let regex = /^(\+7|7|8)?[\s\-]?\(?[489][0-9]{2}\)?[\s\-]?[0-9]{3}[\s\-]?[0-9]{2}[\s\-]?[0-9]{2}$/;
    return regex.test(phone);
}

export function uuid(): string {
    return Math.random().toString(36).substring(2, 15) +
        Math.random().toString(36).substring(2, 15);
}

export function clearNumber(number: string) {
    var clearNumber = number;

    if (clearNumber.startsWith('+7'))
        clearNumber = clearNumber.replace('+7', '8');

    clearNumber = clearNumber.replace('(', '').replace(')', '').replace(' ', '');

    return clearNumber;
}

export function isNumberExists(array: string[], number: string): boolean {
    var clearedNumber = clearNumber(number);

    return array.map(f => clearNumber(f)).indexOf(clearedNumber) > -1;
}