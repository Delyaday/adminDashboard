import { Component, ElementRef, Input, AfterViewInit } from '@angular/core';
import Chart, { ChartConfiguration, ChartConfigurationCustomTypesPerDataset } from 'chart.js/auto';
import { uuid } from '../../utils';



@Component({
    selector: 'chart-component',
    templateUrl: './chart.component.html',
    styleUrls: ['./chart.component.scss'],
})
export class ChartComponent implements AfterViewInit {
    public id: string;
    private _chart: Chart;
    private _canvas: HTMLCanvasElement;
    private _canvasContext: CanvasRenderingContext2D;

    @Input() public options: any;

    constructor(private _element: ElementRef) {
        this.id = uuid();
    }

    ngAfterViewInit(): void {
        this._canvas = document.getElementById(this.id) as HTMLCanvasElement;
        this._canvasContext = this._canvas.getContext("2d") as CanvasRenderingContext2D;

        this._chart = new Chart(this._canvasContext, this.options);
    }

}