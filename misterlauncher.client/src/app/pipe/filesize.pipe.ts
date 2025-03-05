import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'filesize',
  standalone: true
})
export class FilesizePipe implements PipeTransform {

  transform(value: number): string {

    let result = "";
    //console.log(`[Pipe-Filesize] receive ${value} return ${result}`)
    let unit = ["o", "Ko", "Mo", "Go"];
    let i = 0;    
    
    while (value >= 1024 && i < 3) {
      value = value / 1024
      i++;
    }

    result = value.toFixed(value<10 ? 2 : 0) + " " + unit[i];
    //console.log(`[Pipe-Filesize] receive ${value} return ${result}`)

    return result;
  }

}
