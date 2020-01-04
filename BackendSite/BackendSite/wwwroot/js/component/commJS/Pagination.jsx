export class Pagination extends React.Component {
    constructor(props) {
        super(props);

    }

    ChangePage(pageNumber) {
        this.props.ChangePage(pageNumber);
    }

    render() {
        let data = this.props.Data;
        let PageNum=[];

        for(let i = 1; i<=data.totalPages; i++){
            PageNum.push(<li className={"page-item"+(data.pageNumber==i?" active":"")} key={i}><a className="page-link" onClick={data.pageNumber==i ? null: this.ChangePage.bind(this, i)}>{i}</a></li>);
        }

        let disableNext=data.pageNumber==data.totalPages||data.totalPages==0;

        return (
            <nav aria-label="Page navigation example">
                <ul className="pagination justify-content-center">
                    <li className={"page-item"+ (data.pageNumber==1?" disabled":"")}>
                        <a className="page-link" tabIndex="-1" aria-disabled={data.pageNumber==1} onClick={data.pageNumber==1 ? null: this.ChangePage.bind(this, data.pageNumber-1)}>Previous</a>
                    </li>
                    {PageNum}
                    <li className={"page-item"+ (disableNext?" disabled":"")}>
                        <a className="page-link" tabIndex="+1" aria-disabled={disableNext} onClick={disableNext ? null: this.ChangePage.bind(this, data.pageNumber+1)}>Next</a>
                    </li>
                </ul>
            </nav>
        )
    }
}        