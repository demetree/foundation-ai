import { Injectable } from '@angular/core';
import { HttpResponseBase } from '@angular/common/http';


@Injectable()
export class Utilities {

    public static readonly captionAndMessageSeparator = ':';
    public static readonly noNetworkMessageCaption = 'No Network';
    public static readonly noNetworkMessageDetail = 'The server cannot be reached';
    public static readonly accessDeniedMessageCaption = 'Not Found';
    public static readonly accessDeniedMessageDetail = '';


    public static getHttpResponseMessages(data: HttpResponseBase): string[] {
        const responses: string[] = [];

        if (data instanceof HttpResponseBase) {
            if (this.checkNoNetwork(data)) {
                responses.push(`${this.noNetworkMessageCaption}${this.captionAndMessageSeparator} ${this.noNetworkMessageDetail}`);
            } else {
                const responseObject = this.getResponseData(data);

                if (responseObject && typeof responseObject === 'object') {
                    for (const key of Object.keys(responseObject)) {
                        if (key) {
                            responses.push(`${key}${this.captionAndMessageSeparator} ${responseObject[key]}`);
                        } else if (responseObject[key]) {
                            responses.push(responseObject[key].toString());
                        }
                    }
                }
            }

            if (!responses.length) {
                if ((data as any).body) {
                    responses.push(`body: ${(data as any).body}`);
                }
                if ((data as any).error) {
                    responses.push(`error: ${(data as any).error}`);
                }
            }
        }

        if (!responses.length) {
            responses.push(data.toString());
        }

        return responses;
    }


    public static getResponseData(response: HttpResponseBase) {
        let results;

        if (response instanceof HttpResponseBase) {
            if ((response as any).body) {
                results = (response as any).body;
            } else if ((response as any).error) {
                results = (response as any).error;
            }
        }

        return results;
    }


    public static checkNoNetwork(response: HttpResponseBase) {
        if (response instanceof HttpResponseBase) {
            return response.status === 0;
        }
        return false;
    }


    public static checkAccessDenied(response: HttpResponseBase) {
        if (response instanceof HttpResponseBase) {
            return response.status === 403;
        }
        return false;
    }


    public static checkNotFound(response: HttpResponseBase) {
        if (response instanceof HttpResponseBase) {
            return response.status === 404;
        }
        return false;
    }


    public static splitInTwo(text: string, separator: string, splitFromEnd = false): { firstPart: string, secondPart: string | undefined } {
        let separatorIndex: number;

        if (splitFromEnd) {
            separatorIndex = text.lastIndexOf(separator);
        } else {
            separatorIndex = text.indexOf(separator);
        }

        if (separatorIndex === -1) {
            return { firstPart: text, secondPart: undefined };
        }

        const part1 = text.substring(0, separatorIndex).trim();
        const part2 = text.substring(separatorIndex + 1).trim();

        return { firstPart: part1, secondPart: part2 };
    }


    public static getQueryParamsFromString(paramString: string) {
        const params: { [key: string]: string } = {};
        for (const param of paramString.split('&')) {
            const keyValue = Utilities.splitInTwo(param, '=');
            params[keyValue.firstPart] = keyValue.secondPart ?? '';
        }
        return params;
    }


    public static JsonTryParse(value: string) {
        try {
            return JSON.parse(value);
        } catch (e) {
            if (value === 'undefined') { return undefined; }
            return value;
        }
    }


    public static TestIsObjectEmpty(obj: object) {
        for (const prop in obj) {
            if (Object.prototype.hasOwnProperty.call(obj, prop)) {
                return false;
            }
        }
        return true;
    }
}
