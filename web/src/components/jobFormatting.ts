import type { Job } from '../api/types';

const labels: Record<string, string> = {
  FullTime: 'Full time',
  PartTime: 'Part time',
  OnSite: 'On-site',
};

export function readableLabel(value: string) {
  return labels[value] ?? value.replace(/([a-z])([A-Z])/g, '$1 $2');
}

export function formatSalary(job: Job) {
  if (job.salaryMinimum === null && job.salaryMaximum === null) return 'Salary not listed';
  const currency = job.salaryCurrency ?? '';
  const range = [job.salaryMinimum, job.salaryMaximum]
    .filter((value): value is number => value !== null)
    .map((value) => new Intl.NumberFormat('en', { maximumFractionDigits: 0 }).format(value))
    .join(' – ');
  return `${currency} ${range}`.trim();
}
