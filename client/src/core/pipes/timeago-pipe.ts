import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'timeago'
})
export class TimeagoPipe implements PipeTransform {

  transform(value: string | Date): string {
    if (!value) {
      return '';
    }

    const date = new Date(value);
    const now = new Date();
    const seconds = Math.floor((now.getTime() - date.getTime()) / 1000);

    // Define intervals and their corresponding units
    const intervals = {
      year: 31536000,
      month: 2592000,
      week: 604800,
      day: 86400,
      hour: 3600,
      minute: 60,
      second: 1
    };

    if (seconds < 30) {
      return 'just now';
    }

    for (const unit in intervals) {
      const interval = intervals[unit as keyof typeof intervals];
      const counter = Math.floor(seconds / interval);
      if (counter > 0) {
        return `${counter} ${unit}${counter > 1 ? 's' : ''} ago`;
      }
    }

    return date.toLocaleDateString(); // Fallback if no match
  }

}
