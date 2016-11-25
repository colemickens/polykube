export class Counter {
  constructor(
    public hostname: string,
    public instance_count: number,
    public global_count: number,
    public shade: string
  ) {}
}