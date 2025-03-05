import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'timeofday',
  standalone : true
})
export class TimeofdayPipe implements PipeTransform {

  formatter = new Intl.DateTimeFormat('fr', { month: 'short' });

  transform(datetoformat?: Date): string {
    if (datetoformat == undefined) {
      return "unknow";
    }
    let d = new Date(datetoformat);
    let month = d.getMonth();
    let day = d.getDate();
    let year = d.getFullYear();


    let today = new Date();

    if (year != today.getFullYear()) {
      return this.formatter.format(d) + " " + year.toString();
    }

    if (month != today.getMonth()) {
      return this.formatter.format(d)
    }

    if (day != today.getDate()) {
      return d.getDate().toString() + " " + this.formatter.format(d)
    }

    return ("0" + d.getHours()).slice(-2) + "h" + ("0" + d.getMinutes()).slice(-2);
     
    
  }

}
