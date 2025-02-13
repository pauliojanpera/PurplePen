﻿class CanvasDrawer {
    private _ctx: CanvasRenderingContext2D;
    private readonly _argTerminator = ";";
    private readonly _cmdTerminator = "|";
    private _commandList: string;
    private _currentCommandIndex: number;

    constructor(ctx: CanvasRenderingContext2D) {
        this._ctx = ctx;
    }

    getNextArg(): string {
        var endIndex: number = this._commandList.indexOf(this._argTerminator, this._currentCommandIndex);
        if (endIndex < 0) {
            console.log("Missing argument at index " + this._currentCommandIndex);
            return null;
        }

        var arg: string = this._commandList.slice(this._currentCommandIndex, endIndex);
        this._currentCommandIndex = endIndex + 1;
        return arg;
    }

    getFloatArg(): number {
        var arg = this.getNextArg();
        var floatArg: number = Number(arg);
        if (isNaN(floatArg)) {
            console.log("Expected a number argument, got '" + arg + "'");
        }
        return floatArg;
    }

    getIntArg(): number {
        var arg = this.getNextArg();
        var intArg: number = parseInt(arg);
        if (isNaN(intArg)) {
            console.log("Expected a number argument, got '" + arg + "'");
        }
        return intArg;
    }

    getStringArg(): string {
        return this.getNextArg();
    }

    save() {
        this._ctx.save();
    }

    restore() {
        this._ctx.restore();
    }

    fillStyle() {
        var color: string = this.getStringArg();
        this._ctx.fillStyle = color;
    }

    lineStyle() {
        var width: number = this.getFloatArg();
        var color: string = this.getStringArg();
        var join: string = this.getStringArg();
        var cap: string = this.getStringArg();
        var miterLimit: number = this.getFloatArg();
        this._ctx.strokeStyle = color;
        this._ctx.lineWidth = width;
        this._ctx.lineJoin = <CanvasLineJoin>join;
        this._ctx.lineCap = <CanvasLineCap>cap;
        this._ctx.miterLimit = miterLimit;
    }

    drawRect() {
        var x1: number = this.getFloatArg();
        var y1: number = this.getFloatArg();
        var x2: number = this.getFloatArg();
        var y2: number = this.getFloatArg();
        this._ctx.strokeRect(x1, y1, x2 - x1, y2 - y1);
    }

    fillRect() {
        var x1: number = this.getFloatArg();
        var y1: number = this.getFloatArg();
        var x2: number = this.getFloatArg();
        var y2: number = this.getFloatArg();
        this._ctx.fillRect(x1, y1, x2 - x1, y2 - y1);
    }

    drawEllipse() {
        var x1: number = this.getFloatArg();
        var y1: number = this.getFloatArg();
        var x2: number = this.getFloatArg();
        var y2: number = this.getFloatArg();
        this._ctx.beginPath();
        this._ctx.ellipse((x1 + x2) / 2, (y1 + y2) / 2, (x2 - x1) / 2, (y2 - y1) / 2, 0, 0, 2 * Math.PI);
        this._ctx.stroke();
    }

    fillEllipse() {
        var x1: number = this.getFloatArg();
        var y1: number = this.getFloatArg();
        var x2: number = this.getFloatArg();
        var y2: number = this.getFloatArg();
        this._ctx.beginPath();
        this._ctx.ellipse((x1 + x2) / 2, (y1 + y2) / 2, (x2 - x1) / 2, (y2 - y1) / 2, 0, 0, 2 * Math.PI);
        this._ctx.fill();
    }

    arc() {
        var x: number = this.getFloatArg();
        var y: number = this.getFloatArg();
        var r: number = this.getFloatArg();
        var startAngle: number = this.getFloatArg();
        var endAngle: number = this.getFloatArg();
        this._ctx.beginPath();
        this._ctx.arc(x, y, r, startAngle, endAngle);
        this._ctx.stroke();
    }

    beginPath() {
        this._ctx.beginPath();
    }

    closePath() {
        this._ctx.closePath();
    }

    fillPath() {
        var fillRule: number = this.getIntArg();
        if (fillRule != 0) {
            this._ctx.fill("nonzero");
        }
        else {
            this._ctx.fill("evenodd");
        }
    }

    drawPath() {
        this._ctx.stroke();
    }

    clipPath() {
        var fillRule: number = this.getIntArg();

        if (fillRule != 0) {
            this._ctx.clip("nonzero");
        }
        else {
            this._ctx.clip("evenodd");
        }
    }

    moveTo() {
        var x: number = this.getFloatArg();
        var y: number = this.getFloatArg();
        this._ctx.moveTo(x, y);
    }

    lineTo() {
        var x: number = this.getFloatArg();
        var y: number = this.getFloatArg();
        this._ctx.lineTo(x, y);
    }

    bezierTo() {
        var x1: number = this.getFloatArg();
        var y1: number = this.getFloatArg();
        var x2: number = this.getFloatArg();
        var y2: number = this.getFloatArg();
        var x3: number = this.getFloatArg();
        var y3: number = this.getFloatArg();
        this._ctx.bezierCurveTo(x1, y1, x2, y2, x3, y3);
    }

    transform() {
        var m11: number = this.getFloatArg();
        var m12: number = this.getFloatArg();
        var m21: number = this.getFloatArg();
        var m22: number = this.getFloatArg();
        var mdx: number = this.getFloatArg();
        var mdy: number = this.getFloatArg();
        this._ctx.transform(m11, m12, m21, m22, mdx, mdy);
    }

    drawSingleCommand() {
        var whichCommand = this._commandList.charAt(this._currentCommandIndex);
        this._currentCommandIndex += 1;

        switch (whichCommand) {
            case 'S':
                this.fillStyle();
                break;
            case 's':
                this.lineStyle();
                break;
            case 'r':
                this.drawRect();
                break;
            case 'R':
                this.fillRect();
                break;
            case 'e':
                this.drawEllipse();
                break;
            case 'E':
                this.fillEllipse();
                break;
            case 'a':
                this.arc();
                break;
            case 'p':
                this.beginPath();
                break;
            case 'c':
                this.closePath();
                break;
            case 'm':
                this.moveTo();
                break;
            case 'l':
                this.lineTo();
                break;
            case 'b':
                this.bezierTo();
                break;
            case 'f':
                this.fillPath();
                break;
            case 'd':
                this.drawPath();
                break;
            case 'C':
                this.clipPath();
                break;
            case 'x':
                this.transform();
                break;
            case 'z':
                this.save();
                break;
            case 'Z':
                this.restore();
                break;
        }

        // Move to beginning of next command.
        this._currentCommandIndex = this._commandList.indexOf(this._cmdTerminator, this._currentCommandIndex);
        if (this._currentCommandIndex > 0) {
            this._currentCommandIndex += 1;
        }
    }

    drawCommands(commands: string): void {
        this._commandList = commands;
        this._currentCommandIndex = 0;

        this._ctx.save();

        while (true) {
            if (this._currentCommandIndex < 0 || this._currentCommandIndex >= this._commandList.length)
                break;

            this.drawSingleCommand();
        }

        this._ctx.restore();
    }
}

class ParseOnlyCanvasDrawer {
    private readonly _argTerminator = ";";
    private readonly _cmdTerminator = "|";
    private _commandList: string;
    private _currentCommandIndex: number;

    constructor() {

    }

    getNextArg(): string {
        var endIndex: number = this._commandList.indexOf(this._argTerminator, this._currentCommandIndex);
        if (endIndex < 0) {
            console.log("Missing argument at index " + this._currentCommandIndex);
            return null;
        }

        var arg: string = this._commandList.slice(this._currentCommandIndex, endIndex);
        this._currentCommandIndex = endIndex + 1;
        return arg;
    }

    getFloatArg(): number {
        var arg = this.getNextArg();
        var floatArg: number = Number(arg);
        if (isNaN(floatArg)) {
            console.log("Expected a number argument, got '" + arg + "'");
        }
        return floatArg;
    }

    getIntArg(): number {
        var arg = this.getNextArg();
        var intArg: number = parseInt(arg);
        if (isNaN(intArg)) {
            console.log("Expected a number argument, got '" + arg + "'");
        }
        return intArg;
    }

    getStringArg(): string {
        return this.getNextArg();
    }

    save() {
        //this._ctx.save();
    }

    restore() {
        //this._ctx.restore();
    }

    fillStyle() {
        var color: string = this.getStringArg();
        //this._ctx.fillStyle = color;
    }

    lineStyle() {
        var width: number = this.getFloatArg();
        var color: string = this.getStringArg();
        var join: string = this.getStringArg();
        var cap: string = this.getStringArg();
        var miterLimit: number = this.getFloatArg();
        //this._ctx.strokeStyle = color;
        //this._ctx.lineWidth = width;
        //this._ctx.lineJoin = <CanvasLineJoin>join;
        //this._ctx.lineCap = <CanvasLineCap>cap;
        //this._ctx.miterLimit = miterLimit;
    }

    drawRect() {
        var x1: number = this.getFloatArg();
        var y1: number = this.getFloatArg();
        var x2: number = this.getFloatArg();
        var y2: number = this.getFloatArg();
        //this._ctx.strokeRect(x1, y1, x2 - x1, y2 - y1);
    }

    fillRect() {
        var x1: number = this.getFloatArg();
        var y1: number = this.getFloatArg();
        var x2: number = this.getFloatArg();
        var y2: number = this.getFloatArg();
        //this._ctx.fillRect(x1, y1, x2 - x1, y2 - y1);
    }

    drawEllipse() {
        var x1: number = this.getFloatArg();
        var y1: number = this.getFloatArg();
        var x2: number = this.getFloatArg();
        var y2: number = this.getFloatArg();
        //this._ctx.beginPath();
        //this._ctx.ellipse((x1 + x2) / 2, (y1 + y2) / 2, (x2 - x1) / 2, (y2 - y1) / 2, 0, 0, 2 * Math.PI);
        //this._ctx.stroke();
    }

    fillEllipse() {
        var x1: number = this.getFloatArg();
        var y1: number = this.getFloatArg();
        var x2: number = this.getFloatArg();
        var y2: number = this.getFloatArg();
        //this._ctx.beginPath();
        //this._ctx.ellipse((x1 + x2) / 2, (y1 + y2) / 2, (x2 - x1) / 2, (y2 - y1) / 2, 0, 0, 2 * Math.PI);
        //this._ctx.fill();
    }

    arc() {
        var x: number = this.getFloatArg();
        var y: number = this.getFloatArg();
        var r: number = this.getFloatArg();
        var startAngle: number = this.getFloatArg();
        var endAngle: number = this.getFloatArg();
        //this._ctx.beginPath();
        //this._ctx.arc(x, y, r, startAngle, endAngle);
        //this._ctx.stroke();
    }

    beginPath() {
        //this._ctx.beginPath();
    }

    closePath() {
        //this._ctx.closePath();
    }

    fillPath() {
        var fillRule: number = this.getIntArg();
        if (fillRule != 0) {
            //this._ctx.fill("nonzero");
        }
        else {
            //this._ctx.fill("evenodd");
        }
    }

    drawPath() {
        //this._ctx.stroke();
    }

    clipPath() {
        var fillRule: number = this.getIntArg();

        if (fillRule != 0) {
            //this._ctx.clip("nonzero");
        }
        else {
            //this._ctx.clip("evenodd");
        }
    }

    moveTo() {
        var x: number = this.getFloatArg();
        var y: number = this.getFloatArg();
        //this._ctx.moveTo(x, y);
    }

    lineTo() {
        var x: number = this.getFloatArg();
        var y: number = this.getFloatArg();
        //this._ctx.lineTo(x, y);
    }

    bezierTo() {
        var x1: number = this.getFloatArg();
        var y1: number = this.getFloatArg();
        var x2: number = this.getFloatArg();
        var y2: number = this.getFloatArg();
        var x3: number = this.getFloatArg();
        var y3: number = this.getFloatArg();
        //this._ctx.bezierCurveTo(x1, y1, x2, y2, x3, y3);
    }

    transform() {
        var m11: number = this.getFloatArg();
        var m12: number = this.getFloatArg();
        var m21: number = this.getFloatArg();
        var m22: number = this.getFloatArg();
        var mdx: number = this.getFloatArg();
        var mdy: number = this.getFloatArg();
        //this._ctx.transform(m11, m12, m21, m22, mdx, mdy);
    }

    drawSingleCommand() {
        var whichCommand = this._commandList.charAt(this._currentCommandIndex);
        this._currentCommandIndex += 1;

        switch (whichCommand) {
            case 'S':
                this.fillStyle();
                break;
            case 's':
                this.lineStyle();
                break;
            case 'r':
                this.drawRect();
                break;
            case 'R':
                this.fillRect();
                break;
            case 'e':
                this.drawEllipse();
                break;
            case 'E':
                this.fillEllipse();
                break;
            case 'a':
                this.arc();
                break;
            case 'p':
                this.beginPath();
                break;
            case 'c':
                this.closePath();
                break;
            case 'm':
                this.moveTo();
                break;
            case 'l':
                this.lineTo();
                break;
            case 'b':
                this.bezierTo();
                break;
            case 'f':
                this.fillPath();
                break;
            case 'd':
                this.drawPath();
                break;
            case 'C':
                this.clipPath();
                break;
            case 'x':
                this.transform();
                break;
            case 'z':
                this.save();
                break;
            case 'Z':
                this.restore();
                break;
        }

        // Move to beginning of next command.
        this._currentCommandIndex = this._commandList.indexOf(this._cmdTerminator, this._currentCommandIndex);
        if (this._currentCommandIndex > 0) {
            this._currentCommandIndex += 1;
        }
    }

    drawCommands(commands: string): void {
        this._commandList = commands;
        this._currentCommandIndex = 0;

        //this._ctx.save(); 

        while (true) {
            if (this._currentCommandIndex < 0 || this._currentCommandIndex >= this._commandList.length)
                break;

            this.drawSingleCommand();
        }

        //this._ctx.restore();
    }
}

$(document).ready(function () {
    var canvas: HTMLCanvasElement = <HTMLCanvasElement>document.getElementById("canvas");
    var ctx: CanvasRenderingContext2D = canvas.getContext("2d");
    var drawer: CanvasDrawer = new CanvasDrawer(ctx);
    var parser: ParseOnlyCanvasDrawer = new ParseOnlyCanvasDrawer();

    $("#button1").click(async function () {
        var startDownload = performance.now();
        var commandString: string = await $.get("/Home/TestDrawMap");
        var endDownload = performance.now();

        var startRender = performance.now();
        ctx.clearRect(0, 0, canvas.width, canvas.height)
        drawer.drawCommands(commandString);
        var endRender = performance.now();

        var startParse = performance.now();
        parser.drawCommands(commandString);
        var endParse = performance.now();

        $("#timingoutput").html("Done. Time to download: " + (endDownload - startDownload) + "ms. Time to render: " + (endRender - startRender) + "ms. " + " Time to parse: " + (endParse - startParse) + "ms.");
    });
});
