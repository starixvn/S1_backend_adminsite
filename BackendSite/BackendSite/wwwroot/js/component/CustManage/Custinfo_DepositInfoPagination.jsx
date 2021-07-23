import { number_format } from '../commJS/common';
import ReactPaginate from 'react-paginate';

export class DepositInfoPagination extends React.Component {
    constructor(props) {
        super(props);

    }

    ChangePage(pageNumber) {
        const selectedPage = pageNumber.selected;
        this.props.ChangePage(selectedPage+1);
    }

    render() {
        let data = this.props.Data;
        let PageNum=[];

        for(let i = 1; i<=data.totalPages; i++){
            PageNum.push(<li className={"page-item"+(data.pageNumber==i?" active":"")} key={i}><a className="page-link" onClick={data.pageNumber==i ? null: this.ChangePage.bind(this, i)}>{i}</a></li>);
        }

        let disableNext=data.pageNumber==data.totalPages||data.totalPages==0;

        return (
            <nav className="paging" aria-label="Page navigation example">
                <span>
                    <tr>Total approved amount: {number_format(data.totalAmount)}</tr>  
                    <tr>Total records: {data.rowCount}</tr>                             
                </span>
                <ReactPaginate
                    previousLabel={"<"}
                    nextLabel={">"}
                    breakLabel={"..."}
                    breakClassName={"page-item"}
                    breakLinkClassName={"page-link"}
                    pageCount={data.totalPages}
                    marginPagesDisplayed={2}
                    pageRangeDisplayed={10}
                    onPageChange={this.ChangePage.bind(this)}
                    containerClassName={"pagination"}
                    activeClassName={"page-item active"}
                    activeLinkClassName={"page-link"}
                    pageClassName={"page-item"}
                    pageLinkClassName={"page-link"}
                    nextClassName={"page-item"}
                    nextLinkClassName={"page-link"}
                    previousClassName={"page-item"}
                    previousLinkClassName={"page-link"}
                    forcePage={data.pageNumber-1}
                />
            </nav>
        )
    }
}        