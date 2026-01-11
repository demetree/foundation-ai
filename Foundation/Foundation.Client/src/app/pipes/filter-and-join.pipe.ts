import { Pipe, PipeTransform } from '@angular/core';


///
/// The purpose of this pipe is to take in an array of strings, and turn it into a single comma joined string.
///
/// It will remove null and undefined strings from the output
///
@Pipe({ name: 'filterAndJoin' })
export class FilterAndJoinPipe implements PipeTransform {
  transform(values: (string | null | undefined)[], separator: string = ", "): string {
    return values.filter(value => value).join(separator);
  }
}
