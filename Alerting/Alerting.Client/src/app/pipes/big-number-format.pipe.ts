import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'bigNumberFormat',
})
export class BigNumberFormatPipe implements PipeTransform {
  transform(value: number | bigint | null, digitsInfo: string = '1.0-2', locale: string = 'en-US'): string | null {
    if (value == null) {
      return null; // Handle null or undefined input
    }

    if (typeof value === 'bigint') {
      value = Number(value); // Convert bigint to number safely
    }

    // Parse digitsInfo (e.g., "1.0-2")
    const [minInt, minFraction = 0, maxFraction = 3] = digitsInfo.split(/[.-]/).map(Number);

    // Use Intl.NumberFormat for formatting
    const options: Intl.NumberFormatOptions = {
      minimumIntegerDigits: minInt,
      minimumFractionDigits: minFraction,
      maximumFractionDigits: maxFraction,
    };

    return new Intl.NumberFormat(locale, options).format(value);
  }
}
