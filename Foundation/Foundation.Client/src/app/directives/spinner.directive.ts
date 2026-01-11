import { Directive, Input, TemplateRef, ViewContainerRef, ComponentFactoryResolver, ComponentRef } from '@angular/core';
import { SpinnerComponent } from './spinner.component';

@Directive({
  selector: '[showSpinner]'
})
export class SpinnerDirective {
  private spinnerRef: ComponentRef<SpinnerComponent> | null = null;

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private componentFactoryResolver: ComponentFactoryResolver
  ) { }

  @Input() set showSpinner(loading: boolean | null | undefined) {
    this.viewContainer.clear();

    if (loading) {
      this.showSpinnerComponent();
    } else {
      this.showContentComponent();
    }
  }

  private showSpinnerComponent() {
    const factory = this.componentFactoryResolver.resolveComponentFactory(SpinnerComponent);
    this.spinnerRef = this.viewContainer.createComponent(factory);
    this.spinnerRef.instance.loading = true;
  }

  private showContentComponent() {
    if (this.spinnerRef) {
      this.spinnerRef.destroy();
      this.spinnerRef = null;
    }
    this.viewContainer.createEmbeddedView(this.templateRef);
  }
}
