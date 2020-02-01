 let __elem = null;
 let openCnt=0;

 class Loading extends React.Component {
    constructor(props) {
        super(props);
        this.state={show:false};
    }

    componentDidMount() {
        this.setState({show:true});
    }

    render() {
        return (
            <div className="loading-process">
                <div className="load-block">                
                    <div className="line"></div>
                    <div className="line"></div>
                    <div className="line"></div>
                </div>
            </div>
        )
    }
}

let OnLoading = function() {
    ++openCnt;    
    if(__elem == null) {
        __elem = document.createElement('div');
        document.body.appendChild(__elem);
        ReactDOM.render(<Loading/>, __elem);
    }
};

let UnLoading = function() {
    if(openCnt > 0) {
        --openCnt;
    }

    if(openCnt == 0 && __elem!=null) {
        if(ReactDOM.unmountComponentAtNode(__elem)){
            __elem = null;
        }
    }
};

export { OnLoading, UnLoading }; 