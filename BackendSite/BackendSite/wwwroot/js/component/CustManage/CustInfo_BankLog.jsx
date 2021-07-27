export class BankLog extends React.Component {
    constructor(props) {
        super(props);
    }
    componentDidMount() {
        this.props.GetBankLog();
    }
    render() {
        let rows = this.props.searchData.length != 0 ? this.props.searchData.map((row, idx) => {
            return (
                <tr key={"data-" + idx}>
                    <th>{row.bankName}</th>
                    <th>{row.accountName}</th>
                    <th>{row.accountNum}</th>
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
