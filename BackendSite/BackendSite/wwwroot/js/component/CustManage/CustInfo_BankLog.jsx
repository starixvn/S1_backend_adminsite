export class BankLog extends React.Component {
    constructor(props) {
        super(props);
    }
    componentDidMount() {
        this.props.GetBankLog();
    }
    render() {
        
        const actionTypeStatus = {
            1: "Create",
            2: "Delete",
            3: "Block",
            4: "Unlock",
            5: "Bo-Delete"
        }
        
        let rows = this.props.searchData.length != 0 ? this.props.searchData.map((row, idx) => {
            return (
                <tr key={"data-" + idx}>
                    <th>{row.bankName}</th>
                    <th>{row.accountName}</th>
                    <th>{row.accountNum}</th>
                    { this.props.CurrencyId == 61 ?
                        <th>{row.ifscCode}</th>
                    :null}
                    <th>{actionTypeStatus[row.status]}</th>
                    <td>{moment.utc(row.createTime).add(8,'hour').format('l LTS')}</td>
                </tr>
            );
        }) : null;

        return (
            <div className="table" >
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th scope="col">BankName</th>
                            <th scope="col">AccountName</th>
                            <th scope="col">AccountNum</th>
                            { this.props.CurrencyId == 61 ?
                                <th scope="col">IFSC Code</th>
                            :null}
                            <th scope="col">Status</th>
                            <th scope="col">Create Time (Adjusted to GMT+8)</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows}
                    </tbody>
                </table>
            </div>
        )
    }
}
