function Left(input: string, length: number): string {
    return input.substring(0, length);
}

function Right(input: string, length: number): string {
    return input.substring(input.length - length - 1);
}

function Mid(input: string, start: number, length: number): string {
    if (typeof length === 'undefined') {
        return input.substr(start - 1);
    }
    return input.substr(start - 1, length);
}

function UCase(input: string): string {
    return input.toUpperCase();
}

function LCase(input: string): string {
    return input.toLowerCase();
}

function InStr(arg1, arg2, arg3): number {
    var input, search;
    if (typeof arg3 === 'undefined') {
        input = arg1;
        search = arg2;
        return input.indexOf(search) + 1;
    }
    
    var start = arg1;
    input = arg2;
    search = arg3;
    return input.indexOf(search, start - 1) + 1;
}

function Split(input: string, splitChar: string): string[] {
    return input.split(splitChar);
}

function Join(input: string[], joinChar: string): string {
    return input.join(joinChar);
}

function IsNumeric(input): boolean {
    return !isNaN(parseFloat(input)) && isFinite(input);
}

function Replace(input: string, oldString: string, newString: string): string {
    return input.split(oldString).join(newString);
}

function Trim(input: string): string {
    return input.trim();
}

function LTrim(input: string): string {
    return input.replace(/^\s+/,"");
}

function Asc(input: string): number {
    return input.charCodeAt(0);
}

function Chr(input: number): string {
    return String.fromCharCode(input);
}

function Len(input: string): number {
    return input.length;
}

function UBound(array: any[]): number {
    return array.length - 1;
}