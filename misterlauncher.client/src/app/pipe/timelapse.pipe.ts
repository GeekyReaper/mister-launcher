import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'timelapse',
  standalone: true
})
export class TimelapsePipe implements PipeTransform {

  transform(timelapse?: number): string {

    if (timelapse == undefined) {
      return "";
    }

    let time = Math.floor(timelapse / 1000);

    let hours = Math.floor(time / 3600);
    let minutes = Math.floor((time - (hours * 3600)) / 60);
    let secondes = time - (hours * 3600) - (minutes * 60)

    let result = "";
    if (hours > 0) {
      result = hours + "h ";
      if (minutes > 0) {
        result += minutes + "m "
      }
      return result;
    }

    if (minutes > 0) {
      result = minutes + "m ";
      if (secondes > 0) {
        result += secondes + "s";
      }
      return result;
    }

    return secondes + "s";

  }

}
