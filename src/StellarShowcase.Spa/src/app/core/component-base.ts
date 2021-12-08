export class ComponentBase {
    isLoading: boolean;
    isSubmitting: boolean;

    constructor() {
        this.isLoading = true;
    }

    protected stopLoading() {
        setTimeout(() => this.isLoading = false, 600);
    }

    protected stopSubmitting() {
        setTimeout(() => this.isSubmitting = false, 600);
    }
}