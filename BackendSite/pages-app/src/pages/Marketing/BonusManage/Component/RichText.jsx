import React from 'react';

//<RichText EditDetail={EditDetail} Value={props.state.specific[props.currencyId].detail} CurrencyId={props.currencyId}/>

class App extends React.Component {
    constructor(props){
        super(props);
        this.state = {CurrencyId:this.props.CurrencyId};
    }
    static getDerivedStateFromProps(props, state){
        if(props.CurrencyId !== state.CurrencyId){
            let _funcOnChange = props.EditDetail;
            let value = props.Value ?? "";
            /* eslint-disable */
            tinymce.remove("#richText");
            tinymce.init({
                selector: '#richText',
                plugins:['image code textcolor colorpicker help hr link lists preview table fullscreen'],
                menubar: false,
                toolbar:['bold italic underline strikethrough | fontselect fontsizeselect forecolor backcolor | alignleft aligncenter alignright alignjustify | numlist bullist | table | link image hr | code preview | help fullscreen'],
                height : 300,
                setup: function(editor) {
                    editor.on('init', function() {
                            this.setContent(value);
                    });
                },
                init_instance_callback: function (editor) {
                    editor.on('Change', function(e){
                        let v = tinyMCE.activeEditor.getContent();
                        _funcOnChange(v);
                    });
                }
            });
            /* eslint-enable */
            return {CurrencyId: props.CurrencyId}
        }else{
            return null;
        }
    }
    componentDidMount(){
        let _funcOnChange = this.props.EditDetail;
        let value = this.props.Value ?? "";
        /* eslint-disable */
        tinymce.remove("#richText");
        tinymce.init({
            selector: '#richText',
            plugins:['image code textcolor colorpicker help hr link lists preview table fullscreen'],
            menubar: false,
            toolbar:['bold italic underline strikethrough | fontselect fontsizeselect forecolor backcolor | alignleft aligncenter alignright alignjustify | numlist bullist | table | link image hr | code preview | help fullscreen'],
            height : 300,
            setup: function(editor) {
                editor.on('init', function() {
                        this.setContent(value);
                });
            },
            init_instance_callback: function (editor) {
                editor.on('Change', function(e){
                    let v = tinyMCE.activeEditor.getContent();
                    _funcOnChange(v);
                });
            }
        });
        /* eslint-enable */
    }
    render() {
        return (
            <div className="form-group">
                <label>Detail</label>
                <textarea
                    id="richText"
                    className="form-control"
                    rows="8"
                    onChange={this.props.EditDetail}
                    value={this.props.Value}></textarea>
            </div>
        );
    }
}

export default App;